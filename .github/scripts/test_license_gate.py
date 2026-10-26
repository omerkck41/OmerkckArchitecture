"""Tests for license_gate.py. Run: python -I -m unittest discover -s .github/scripts -p "test_*.py"."""
import datetime
import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import license_gate  # noqa: E402

NUSPEC = """<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata>
    <id>{id}</id>
    <version>{version}</version>
    {license}
  </metadata>
</package>
"""

TODAY = datetime.date(2026, 10, 26)


class GateFixture:
    """Builds a fake repository (lock files) and a fake NuGet global packages folder."""

    def __init__(self) -> None:
        self._tmp = tempfile.TemporaryDirectory()
        base = Path(self._tmp.name)
        self.repo = base / "repo"
        self.nuget = base / "nuget"
        self.repo.mkdir()
        self.nuget.mkdir()
        self._deps: dict[str, dict] = {}

    def close(self) -> None:
        self._tmp.cleanup()

    def add(self, pkg_id: str, version: str, license_xml: str = "", files: dict[str, str] | None = None,
            dep_type: str = "Direct", write_nuspec: bool = True, raw_nuspec: str | None = None) -> None:
        self._deps[pkg_id] = {"type": dep_type, "resolved": version}
        if dep_type == "Project":
            return
        pkg_dir = self.nuget / pkg_id.lower() / version.lower()
        pkg_dir.mkdir(parents=True)
        if write_nuspec:
            text = raw_nuspec if raw_nuspec is not None else NUSPEC.format(id=pkg_id, version=version, license=license_xml)
            (pkg_dir / f"{pkg_id.lower()}.nuspec").write_text(text, encoding="utf-8")
        for name, content in (files or {}).items():
            (pkg_dir / name).write_text(content, encoding="utf-8", newline="")

    def write_lock(self, project: str = "src/Lib") -> None:
        lock_dir = self.repo / project
        lock_dir.mkdir(parents=True, exist_ok=True)
        lock = {"version": 2, "dependencies": {"net10.0": self._deps}}
        (lock_dir / "packages.lock.json").write_text(json.dumps(lock), encoding="utf-8")

    def run(self, policy: dict) -> tuple[int, list]:
        packages = license_gate.collect_packages(self.repo)
        findings = license_gate.evaluate(packages, self.nuget, policy, TODAY)
        return license_gate.exit_code(findings), findings


def expression(value: str) -> str:
    return f'<license type="expression">{value}</license>'


def file_license(name: str) -> str:
    return f'<license type="file">{name}</license>'


BASE_POLICY = {"allowed": ["MIT", "Apache-2.0", "BSD-3-Clause"], "reviewed": []}


class LicenseGateTests(unittest.TestCase):
    def setUp(self) -> None:
        self.fx = GateFixture()

    def tearDown(self) -> None:
        self.fx.close()

    def statuses(self, findings) -> dict[str, str]:
        return {f.package_id: f.status for f in findings}

    def test_permissive_expression_passes(self) -> None:
        self.fx.add("Good.Lib", "1.0.0", expression("MIT"))
        self.fx.write_lock()
        code, findings = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 0)
        self.assertEqual(self.statuses(findings), {"Good.Lib": "ok"})

    def test_or_expression_with_allowed_alternative_passes(self) -> None:
        self.fx.add("Dual.Lib", "1.0.0", expression("Apache-2.0 OR MPL-2.0"))
        self.fx.write_lock()
        code, _ = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 0)

    def test_copyleft_expression_fails(self) -> None:
        self.fx.add("Gpl.Lib", "1.0.0", expression("GPL-3.0-only"))
        self.fx.write_lock()
        code, findings = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 1)
        self.assertEqual(self.statuses(findings), {"Gpl.Lib": "violation"})

    def test_and_expression_is_not_auto_allowed(self) -> None:
        self.fx.add("And.Lib", "1.0.0", expression("MIT AND GPL-3.0-only"))
        self.fx.write_lock()
        code, _ = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 1)

    def test_malformed_expressions_are_not_auto_allowed(self) -> None:
        allowed = {"MIT", "Apache-2.0"}
        for expr in ("GPL-3.0-only MIT", "MIT OR", "OR MIT", "MIT OR OR Apache-2.0", "MIT or GPL-3.0-only", ""):
            with self.subTest(expression=expr):
                self.assertFalse(license_gate.expression_allowed(expr, allowed))

    def test_well_formed_or_expressions_are_allowed(self) -> None:
        allowed = {"MIT", "Apache-2.0"}
        for expr in ("MIT", "(MIT)", "Apache-2.0 OR MPL-2.0", "(GPL-2.0-only OR MIT)"):
            with self.subTest(expression=expr):
                self.assertTrue(license_gate.expression_allowed(expr, allowed))

    def test_unreviewed_file_license_fails_and_reports_hash(self) -> None:
        self.fx.add("Paid.Lib", "14.1.0", file_license("LICENSE.md"), {"LICENSE.md": "Commercial terms"})
        self.fx.write_lock()
        code, findings = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 1)
        self.assertEqual(findings[0].status, "violation")
        self.assertIn(license_gate.text_sha256("Commercial terms"), findings[0].detail)

    def test_reviewed_file_license_with_matching_hash_passes(self) -> None:
        self.fx.add("FileMit.Lib", "3.0.2", file_license("LICENSE"), {"LICENSE": "MIT License\r\nPermission..."})
        self.fx.write_lock()
        policy = {**BASE_POLICY, "reviewed": [{
            "id": "FileMit.Lib", "kind": "file", "license": "MIT", "status": "allowed",
            "sha256": [license_gate.text_sha256("MIT License\nPermission...")]}]}
        code, findings = self.fx.run(policy)
        self.assertEqual(code, 0)
        self.assertEqual(findings[0].status, "ok")

    def test_reviewed_file_license_with_changed_text_fails(self) -> None:
        self.fx.add("FileMit.Lib", "4.0.0", file_license("LICENSE"), {"LICENSE": "Now commercial"})
        self.fx.write_lock()
        policy = {**BASE_POLICY, "reviewed": [{
            "id": "FileMit.Lib", "kind": "file", "license": "MIT", "status": "allowed",
            "sha256": [license_gate.text_sha256("MIT License\nPermission...")]}]}
        code, _ = self.fx.run(policy)
        self.assertEqual(code, 1)

    def test_active_exception_passes_and_is_marked(self) -> None:
        self.fx.add("Hangfire.Core", "1.8.25", file_license("LICENSE.md"), {"LICENSE.md": "LGPL"})
        self.fx.write_lock()
        policy = {**BASE_POLICY, "reviewed": [{
            "id": "Hangfire*", "kind": "file", "license": "LGPL-3.0", "status": "exception",
            "reason": "dynamic linking", "sha256": [license_gate.text_sha256("LGPL")]}]}
        code, findings = self.fx.run(policy)
        self.assertEqual(code, 0)
        self.assertEqual(findings[0].status, "exception")

    def test_expired_exception_fails(self) -> None:
        self.fx.add("MediatR", "14.1.0", file_license("LICENSE.md"), {"LICENSE.md": "RPL"})
        self.fx.write_lock()
        policy = {**BASE_POLICY, "reviewed": [{
            "id": "MediatR", "kind": "file", "license": "RPL-1.5", "status": "exception",
            "reason": "temporary", "expires": "2026-10-25", "sha256": [license_gate.text_sha256("RPL")]}]}
        code, findings = self.fx.run(policy)
        self.assertEqual(code, 1)
        self.assertIn("expired", findings[0].detail)

    def test_reviewed_url_license_passes(self) -> None:
        url = "https://raw.githubusercontent.com/xunit/xunit/master/license.txt"
        self.fx.add("xunit.abstractions", "2.0.3", f"<licenseUrl>{url}</licenseUrl>")
        self.fx.write_lock()
        policy = {**BASE_POLICY, "reviewed": [{
            "id": "xunit.abstractions", "kind": "url", "license": "Apache-2.0", "status": "allowed", "url": url}]}
        code, _ = self.fx.run(policy)
        self.assertEqual(code, 0)

    def test_unreviewed_url_license_fails(self) -> None:
        self.fx.add("Url.Lib", "1.0.0", "<licenseUrl>https://example.com/eula</licenseUrl>")
        self.fx.write_lock()
        code, _ = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 1)

    def test_missing_nuspec_fails_closed(self) -> None:
        self.fx.add("Ghost.Lib", "1.0.0", write_nuspec=False)
        self.fx.write_lock()
        code, findings = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 1)
        self.assertIn("nuspec", findings[0].detail)

    def test_nuspec_with_dtd_is_rejected(self) -> None:
        raw = '<?xml version="1.0"?><!DOCTYPE x [<!ENTITY a "b">]><package><metadata></metadata></package>'
        self.fx.add("Dtd.Lib", "1.0.0", raw_nuspec=raw)
        self.fx.write_lock()
        code, findings = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 1)
        self.assertIn("DTD", findings[0].detail)

    def test_project_references_are_skipped(self) -> None:
        self.fx.add("Kck.Core.Abstractions", "1.0.0", dep_type="Project")
        self.fx.add("Good.Lib", "1.0.0", expression("MIT"))
        self.fx.write_lock()
        _, findings = self.fx.run(BASE_POLICY)
        self.assertEqual(self.statuses(findings), {"Good.Lib": "ok"})

    def test_lock_files_in_hidden_directories_are_ignored(self) -> None:
        self.fx.add("Good.Lib", "1.0.0", expression("MIT"))
        self.fx.write_lock()
        self.fx.add("Stale.Lib", "0.1.0", expression("GPL-3.0-only"))
        self.fx.write_lock(".claude/worktrees/old/src/Lib")
        code, findings = self.fx.run(BASE_POLICY)
        self.assertEqual(code, 0)
        self.assertEqual(self.statuses(findings), {"Good.Lib": "ok"})

    def test_no_lock_files_fails_closed(self) -> None:
        with self.assertRaises(license_gate.GateError):
            license_gate.collect_packages(self.fx.repo)

    def test_policy_outside_root_is_rejected(self) -> None:
        self.fx.add("Good.Lib", "1.0.0", expression("MIT"))
        self.fx.write_lock()
        outside = self.fx.repo.parent / "policy.json"
        outside.write_text(json.dumps(BASE_POLICY), encoding="utf-8")
        code = license_gate.main(["--root", str(self.fx.repo), "--policy", str(outside),
                                  "--nuget-root", str(self.fx.nuget), "--today", "2026-10-26"])
        self.assertEqual(code, 2)

    def test_report_outside_root_is_rejected(self) -> None:
        self.fx.add("Good.Lib", "1.0.0", expression("MIT"))
        self.fx.write_lock()
        policy = self.fx.repo / "policy.json"
        policy.write_text(json.dumps(BASE_POLICY), encoding="utf-8")
        code = license_gate.main(["--root", str(self.fx.repo), "--policy", str(policy),
                                  "--nuget-root", str(self.fx.nuget), "--report", str(self.fx.repo.parent / "r.md")])
        self.assertEqual(code, 2)

    def test_main_passes_with_paths_inside_root(self) -> None:
        self.fx.add("Good.Lib", "1.0.0", expression("MIT"))
        self.fx.write_lock()
        policy = self.fx.repo / "policy.json"
        policy.write_text(json.dumps(BASE_POLICY), encoding="utf-8")
        code = license_gate.main(["--root", str(self.fx.repo), "--policy", str(policy),
                                  "--nuget-root", str(self.fx.nuget), "--report", str(self.fx.repo / "out" / "r.md")])
        self.assertEqual(code, 0)
        self.assertTrue((self.fx.repo / "out" / "r.md").is_file())

    def test_clean_strips_line_breaks_that_could_inject_workflow_commands(self) -> None:
        self.assertEqual(license_gate.clean("MIT\n::warning::forged\r\x1b"), "MIT ::warning::forged  ")

    def test_unused_reviewed_entries_are_reported(self) -> None:
        self.fx.add("Good.Lib", "1.0.0", expression("MIT"))
        self.fx.write_lock()
        policy = {**BASE_POLICY, "reviewed": [{
            "id": "Gone.Lib", "kind": "file", "license": "MIT", "status": "allowed", "sha256": ["0"]}]}
        packages = license_gate.collect_packages(self.fx.repo)
        findings = license_gate.evaluate(packages, self.fx.nuget, policy, TODAY)
        self.assertEqual(license_gate.unused_entries(policy, findings), ["Gone.Lib"])


if __name__ == "__main__":
    unittest.main()
