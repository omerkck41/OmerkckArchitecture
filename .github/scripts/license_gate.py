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
import re
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path

log = logging.getLogger("license-gate")

SKIPPED_DIRS = {"bin", "obj", "node_modules", ".git"}


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


def collect_packages(repo_root: Path) -> dict[tuple[str, str], set[str]]:
    """Returns {(package id, resolved version): {project names}} for all non-project lock entries."""
    packages: dict[tuple[str, str], set[str]] = {}
    # Hidden directories (.git, .claude worktrees, ...) hold copies of other branches, not this checkout's dependencies.
    lock_files = [p for p in repo_root.rglob("packages.lock.json")
                  if not any(part.startswith(".") or part in SKIPPED_DIRS for part in p.relative_to(repo_root).parts)]
    if not lock_files:
        raise GateError(f"no packages.lock.json found under {repo_root}")
    for lock in lock_files:
        data = json.loads(lock.read_text(encoding="utf-8-sig"))
        for deps in data.get("dependencies", {}).values():
            for pkg_id, info in deps.items():
                if info.get("type") == "Project":
                    continue
                resolved = info.get("resolved")
                if not resolved:
                    raise GateError(f"{lock}: {pkg_id} has no resolved version")
                packages.setdefault((pkg_id, resolved), set()).add(lock.parent.name)
    return packages


def read_license(pkg_dir: Path) -> LicenseInfo:
    """Reads the declared license of an extracted package; problems are returned as kind 'error'."""
    nuspecs = sorted(pkg_dir.glob("*.nuspec"))
    if not nuspecs:
        return LicenseInfo("error", f"nuspec not found in {pkg_dir}")
    raw = nuspecs[0].read_text(encoding="utf-8-sig", errors="replace")
    # Third-party content: refuse DTDs so entity expansion / XXE never reaches the parser.
    if "<!DOCTYPE" in raw or "<!ENTITY" in raw:
        return LicenseInfo("error", "nuspec contains a DTD; refused")
    try:
        root = ET.fromstring(raw)
    except ET.ParseError as exc:
        return LicenseInfo("error", f"nuspec parse error: {exc}")
    metadata = next((el for el in root if el.tag.endswith("metadata")), None)
    if metadata is None:
        return LicenseInfo("error", "nuspec has no metadata element")
    fields = {child.tag.split("}")[-1]: child for child in metadata}
    license_node = fields.get("license")
    if license_node is not None:
        value = (license_node.text or "").strip()
        if license_node.get("type", "") == "file":
            license_file = (pkg_dir / value).resolve()
            if pkg_dir.resolve() not in license_file.parents or not license_file.is_file():
                return LicenseInfo("error", f"license file '{value}' missing")
            text = license_file.read_text(encoding="utf-8-sig", errors="replace")
            return LicenseInfo("file", value, text_sha256(text))
        return LicenseInfo("expression", value)
    url_node = fields.get("licenseUrl")
    if url_node is not None and (url_node.text or "").strip():
        return LicenseInfo("url", url_node.text.strip())
    return LicenseInfo("error", "no license metadata")


def expression_allowed(expression: str, allowed: set[str]) -> bool:
    """True for an allowed SPDX id, or an OR-only expression with at least one allowed alternative."""
    cleaned = expression.replace("(", " ").replace(")", " ").strip()
    if re.search(r"\b(AND|WITH)\b", cleaned, flags=re.IGNORECASE):
        return False
    options = [opt.strip() for opt in re.split(r"\s+OR\s+", cleaned, flags=re.IGNORECASE)]
    return any(opt in allowed for opt in options)


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


def evaluate(packages: dict[tuple[str, str], set[str]], nuget_root: Path, policy: dict,
             today: datetime.date) -> list[Finding]:
    """Classifies every package as ok, exception (documented, not expired) or violation."""
    allowed = set(policy.get("allowed", []))
    reviewed = policy.get("reviewed", [])
    findings: list[Finding] = []
    for (pkg_id, version), projects in sorted(packages.items(), key=lambda kv: (kv[0][0].lower(), kv[0][1])):
        info = read_license(nuget_root / pkg_id.lower() / version.lower())
        project_list = ";".join(sorted(projects))
        if info.kind == "error":
            findings.append(Finding(pkg_id, version, "violation", "UNKNOWN", info.value, projects=project_list))
            continue
        if info.kind == "expression" and expression_allowed(info.value, allowed):
            findings.append(Finding(pkg_id, version, "ok", info.value, "allowed expression", projects=project_list))
            continue
        entry = next((e for e in reviewed if _entry_matches(e, pkg_id, info)), None)
        if entry is None:
            findings.append(Finding(pkg_id, version, "violation", "UNKNOWN",
                                    f"not allowed and not reviewed ({_describe(info)})", projects=project_list))
            continue
        if entry.get("status") == "allowed":
            findings.append(Finding(pkg_id, version, "ok", entry["license"], "reviewed", entry["id"], project_list))
            continue
        expires = entry.get("expires")
        if expires and datetime.date.fromisoformat(expires) < today:
            findings.append(Finding(pkg_id, version, "violation", entry["license"],
                                    f"exception expired on {expires}: {entry.get('reason', '')}", entry["id"],
                                    project_list))
            continue
        until = f" (until {expires})" if expires else ""
        findings.append(Finding(pkg_id, version, "exception", entry["license"],
                                f"documented exception{until}: {entry.get('reason', '')}", entry["id"], project_list))
    return findings


def unused_entries(policy: dict, findings: list[Finding]) -> list[str]:
    """Reviewed entries that matched no package (stale policy lines)."""
    used = {f.entry_id for f in findings if f.entry_id}
    return [e["id"] for e in policy.get("reviewed", []) if e["id"] not in used]


def exit_code(findings: list[Finding]) -> int:
    return 1 if any(f.status == "violation" for f in findings) else 0


def write_report(path: Path, findings: list[Finding], unused: list[str]) -> None:
    """Markdown report (GitHub step summary / artifact)."""
    counts = {s: sum(1 for f in findings if f.status == s) for s in ("ok", "exception", "violation")}
    lines = ["## License gate", "",
             f"Packages: {len(findings)} · ok: {counts['ok']} · exception: {counts['exception']} · "
             f"violation: {counts['violation']}", ""]
    notable = [f for f in findings if f.status != "ok"]
    if notable:
        lines += ["| Status | Package | Version | License | Detail |", "|---|---|---|---|---|"]
        lines += [f"| {f.status} | {f.package_id} | {f.version} | {f.license} | {f.detail} |" for f in notable]
    if unused:
        lines += ["", "Unused policy entries: " + ", ".join(unused)]
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--root", type=Path, default=Path.cwd(), help="repository root")
    parser.add_argument("--policy", type=Path, required=True, help="license policy JSON")
    parser.add_argument("--nuget-root", type=Path,
                        default=Path(os.environ.get("NUGET_PACKAGES") or Path.home() / ".nuget" / "packages"))
    parser.add_argument("--today", type=datetime.date.fromisoformat, default=datetime.date.today())
    parser.add_argument("--report", type=Path, help="write a markdown report here")
    args = parser.parse_args(argv)
    logging.basicConfig(level=logging.INFO, format="%(message)s", stream=sys.stdout)

    try:
        policy = json.loads(args.policy.read_text(encoding="utf-8"))
        findings = evaluate(collect_packages(args.root), args.nuget_root, policy, args.today)
    except (GateError, OSError, ValueError, KeyError) as exc:
        log.error("::error::license gate could not run: %s", exc)
        return 2

    for f in findings:
        if f.status == "violation":
            log.error("::error::%s %s - %s [%s]", f.package_id, f.version, f.detail, f.projects)
        elif f.status == "exception":
            log.warning("::warning::%s %s - %s", f.package_id, f.version, f.detail)
    unused = unused_entries(policy, findings)
    for entry_id in unused:
        log.warning("::warning::policy entry '%s' matched no package", entry_id)
    if args.report:
        write_report(args.report, findings, unused)
    code = exit_code(findings)
    log.info("license gate: %d packages, %d violation(s) -> %s", len(findings),
             sum(1 for f in findings if f.status == "violation"), "FAIL" if code else "PASS")
    return code


if __name__ == "__main__":
    sys.exit(main())
