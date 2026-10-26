"""Fail-closed license gate for NuGet dependencies.

Reads every packages.lock.json under the repository (the exact set CI restores with --locked-mode), looks up each
resolved package's nuspec in the NuGet global packages folder and checks its declared license against a policy file.
Decision record: decisions/2026-10-26-license-gate.md.

Exit codes: 0 = all packages allowed or covered by an active exception, 1 = violations, 2 = gate could not run.
"""
from __future__ import annotations

import argparse
import datetime
import fnmatch
import hashlib
import json
import logging
import os
import sys
import xml.etree.ElementTree as ET
from collections.abc import Iterator
from dataclasses import dataclass
from pathlib import Path

log = logging.getLogger("license-gate")

SKIPPED_DIRS = {"bin", "obj", "node_modules"}


class GateError(Exception):
    """The gate could not evaluate the dependency set (always fails the build)."""


@dataclass(frozen=True)
class LicenseInfo:
    kind: str  # expression | file | url | error
    value: str
    sha256: str = ""


@dataclass(frozen=True)
class Finding:
    package_id: str
    version: str
    status: str  # ok | exception | violation
    license: str
    detail: str
    entry_id: str = ""
    projects: str = ""


def text_sha256(text: str) -> str:
    """Hashes license text with every whitespace run collapsed (line endings, re-wrapping); word changes still count."""
    normalized = " ".join(text.split())
    return hashlib.sha256(normalized.encode("utf-8")).hexdigest()


def clean(value: object) -> str:
    """Replaces non-printable characters so third-party text cannot break log lines or inject workflow commands."""
    return "".join(ch if ch.isprintable() else " " for ch in str(value))


def _lock_files(repo_root: Path) -> list[Path]:
    # Hidden directories (.git, .claude worktrees, ...) hold copies of other branches, not this checkout's dependencies.
    found = []
    for path in repo_root.rglob("packages.lock.json"):
        parts = path.relative_to(repo_root).parts
        if not any(part.startswith(".") or part in SKIPPED_DIRS for part in parts):
            found.append(path)
    return found


def _lock_entries(lock: Path) -> Iterator[tuple[str, str]]:
    data = json.loads(lock.read_text(encoding="utf-8-sig"))
    for deps in data.get("dependencies", {}).values():
        for pkg_id, info in deps.items():
            if info.get("type") == "Project":
                continue
            resolved = info.get("resolved")
            if not resolved:
                raise GateError(f"{lock}: {pkg_id} has no resolved version")
            yield pkg_id, resolved


def collect_packages(repo_root: Path) -> dict[tuple[str, str], set[str]]:
    """Returns {(package id, resolved version): {project names}} for all non-project lock entries."""
    lock_files = _lock_files(repo_root)
    if not lock_files:
        raise GateError(f"no packages.lock.json found under {repo_root}")
    packages: dict[tuple[str, str], set[str]] = {}
    for lock in lock_files:
        for key in _lock_entries(lock):
            packages.setdefault(key, set()).add(lock.parent.name)
    return packages


def _metadata_fields(raw: str) -> dict[str, ET.Element] | str:
    """Parses nuspec text into {local tag name: element}; returns an error message instead of raising."""
    # Third-party content: refuse DTDs so entity expansion / XXE never reaches the parser.
    if "<!DOCTYPE" in raw or "<!ENTITY" in raw:
        return "nuspec contains a DTD; refused"
    try:
        root = ET.fromstring(raw)
    except ET.ParseError as exc:
        return f"nuspec parse error: {exc}"
    metadata = next((el for el in root if el.tag.endswith("metadata")), None)
    if metadata is None:
        return "nuspec has no metadata element"
    return {child.tag.split("}")[-1]: child for child in metadata}


def _file_license(pkg_dir: Path, name: str) -> LicenseInfo:
    license_file = (pkg_dir / name).resolve()
    if pkg_dir.resolve() not in license_file.parents or not license_file.is_file():
        return LicenseInfo("error", f"license file '{name}' missing")
    text = license_file.read_text(encoding="utf-8-sig", errors="replace")
    return LicenseInfo("file", name, text_sha256(text))


def read_license(pkg_dir: Path) -> LicenseInfo:
    """Reads the declared license of an extracted package; problems are returned as kind 'error'."""
    nuspecs = sorted(pkg_dir.glob("*.nuspec"))
    if not nuspecs:
        return LicenseInfo("error", f"nuspec not found in {pkg_dir}")
    fields = _metadata_fields(nuspecs[0].read_text(encoding="utf-8-sig", errors="replace"))
    if isinstance(fields, str):
        return LicenseInfo("error", fields)
    license_node = fields.get("license")
    if license_node is not None:
        value = (license_node.text or "").strip()
        if license_node.get("type", "") == "file":
            return _file_license(pkg_dir, value)
        return LicenseInfo("expression", value)
    url = (fields["licenseUrl"].text or "").strip() if "licenseUrl" in fields else ""
    return LicenseInfo("url", url) if url else LicenseInfo("error", "no license metadata")


def expression_allowed(expression: str, allowed: set[str]) -> bool:
    """True for an allowed SPDX id, or a well-formed OR-only expression with at least one allowed alternative.

    Tokens must strictly alternate id / OR / id (operators are case-sensitive, as in SPDX); anything else — AND, WITH,
    adjacent ids, a dangling or doubled OR — is not auto-allowed and must be reviewed in the policy (fail-closed).
    """
    tokens = expression.replace("(", " ").replace(")", " ").split()
    if len(tokens) % 2 == 0:
        return False
    ids = tokens[0::2]
    operators = tokens[1::2]
    if any(op != "OR" for op in operators) or any(token.upper() in {"AND", "OR", "WITH"} for token in ids):
        return False
    return any(token in allowed for token in ids)


def _entry_matches(entry: dict, pkg_id: str, info: LicenseInfo) -> bool:
    if not fnmatch.fnmatchcase(pkg_id, entry["id"]) or entry.get("kind") != info.kind:
        return False
    if info.kind == "file":
        return info.sha256 in entry.get("sha256", [])
    if info.kind == "url":
        return info.value == entry.get("url")
    if info.kind == "expression":
        return info.value == entry.get("expression")
    return False


def _describe(info: LicenseInfo) -> str:
    if info.kind == "file":
        return f"file:{info.value} sha256={info.sha256}"
    return f"{info.kind}:{info.value}"


def _classify(pkg_id: str, version: str, projects: str, info: LicenseInfo, allowed: set[str], reviewed: list[dict],
              today: datetime.date) -> Finding:
    if info.kind == "error":
        return Finding(pkg_id, version, "violation", "UNKNOWN", info.value, projects=projects)
    if info.kind == "expression" and expression_allowed(info.value, allowed):
        return Finding(pkg_id, version, "ok", info.value, "allowed expression", projects=projects)
    entry = next((e for e in reviewed if _entry_matches(e, pkg_id, info)), None)
    if entry is None:
        return Finding(pkg_id, version, "violation", "UNKNOWN", f"not allowed and not reviewed ({_describe(info)})",
                       projects=projects)
    if entry.get("status") == "allowed":
        return Finding(pkg_id, version, "ok", entry["license"], "reviewed", entry["id"], projects)
    expires = entry.get("expires")
    reason = entry.get("reason", "")
    if expires and datetime.date.fromisoformat(expires) < today:
        return Finding(pkg_id, version, "violation", entry["license"], f"exception expired on {expires}: {reason}",
                       entry["id"], projects)
    until = f" (until {expires})" if expires else ""
    return Finding(pkg_id, version, "exception", entry["license"], f"documented exception{until}: {reason}",
                   entry["id"], projects)


def evaluate(packages: dict[tuple[str, str], set[str]], nuget_root: Path, policy: dict,
             today: datetime.date) -> list[Finding]:
    """Classifies every package as ok, exception (documented, not expired) or violation."""
    allowed = set(policy.get("allowed", []))
    reviewed = policy.get("reviewed", [])
    findings = []
    for (pkg_id, version), projects in sorted(packages.items(), key=lambda kv: (kv[0][0].lower(), kv[0][1])):
        info = read_license(nuget_root / pkg_id.lower() / version.lower())
        findings.append(_classify(pkg_id, version, ";".join(sorted(projects)), info, allowed, reviewed, today))
    return findings


def unused_entries(policy: dict, findings: list[Finding]) -> list[str]:
    """Reviewed entries that matched no package (stale policy lines)."""
    used = {f.entry_id for f in findings if f.entry_id}
    return [e["id"] for e in policy.get("reviewed", []) if e["id"] not in used]


def exit_code(findings: list[Finding]) -> int:
    return 1 if any(f.status == "violation" for f in findings) else 0


def _cell(value: str) -> str:
    return clean(value).replace("|", "\\|")


def write_report(path: Path, findings: list[Finding], unused: list[str]) -> None:
    """Markdown report (GitHub step summary / artifact)."""
    counts = {s: sum(1 for f in findings if f.status == s) for s in ("ok", "exception", "violation")}
    lines = ["## License gate", "",
             f"Packages: {len(findings)} · ok: {counts['ok']} · exception: {counts['exception']} · "
             f"violation: {counts['violation']}", ""]
    notable = [f for f in findings if f.status != "ok"]
    if notable:
        lines += ["| Status | Package | Version | License | Detail |", "|---|---|---|---|---|"]
        lines += [f"| {f.status} | {_cell(f.package_id)} | {_cell(f.version)} | {_cell(f.license)} | {_cell(f.detail)} |"
                  for f in notable]
    if unused:
        lines += ["", "Unused policy entries: " + ", ".join(_cell(u) for u in unused)]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def _inside(root: Path, path: Path, what: str) -> Path:
    """Resolves a CLI path and refuses anything outside the repository root."""
    resolved = path.resolve()
    if resolved != root and root not in resolved.parents:
        raise GateError(f"{what} must be inside the repository root: {resolved}")
    return resolved


def _log_findings(findings: list[Finding], unused: list[str]) -> None:
    for f in findings:
        if f.status == "violation":
            log.error("::error::%s %s - %s [%s]", clean(f.package_id), clean(f.version), clean(f.detail),
                      clean(f.projects))
        elif f.status == "exception":
            log.warning("::warning::%s %s - %s", clean(f.package_id), clean(f.version), clean(f.detail))
    for entry_id in unused:
        log.warning("::warning::policy entry '%s' matched no package", clean(entry_id))


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--root", type=Path, default=Path.cwd(), help="repository root")
    parser.add_argument("--policy", type=Path, required=True, help="license policy JSON (inside --root)")
    parser.add_argument("--nuget-root", type=Path,
                        default=Path(os.environ.get("NUGET_PACKAGES") or Path.home() / ".nuget" / "packages"))
    parser.add_argument("--today", type=datetime.date.fromisoformat, default=datetime.date.today())
    parser.add_argument("--report", type=Path, help="write a markdown report here (inside --root)")
    args = parser.parse_args(argv)
    logging.basicConfig(level=logging.INFO, format="%(message)s", stream=sys.stdout)

    try:
        root = args.root.resolve()
        policy_path = _inside(root, args.policy, "--policy")
        report_path = _inside(root, args.report, "--report") if args.report else None
        policy = json.loads(policy_path.read_text(encoding="utf-8"))
        findings = evaluate(collect_packages(root), args.nuget_root, policy, args.today)
    except (GateError, OSError, ValueError, KeyError) as exc:
        log.exception("::error::license gate could not run: %s", clean(exc))
        return 2

    unused = unused_entries(policy, findings)
    _log_findings(findings, unused)
    if report_path:
        write_report(report_path, findings, unused)
    code = exit_code(findings)
    log.info("license gate: %d packages, %d violation(s) -> %s", len(findings),
             sum(1 for f in findings if f.status == "violation"), "FAIL" if code else "PASS")
    return code


if __name__ == "__main__":
    sys.exit(main())
