"""Tests for the version selection in nuget-publish.yml "Unlist older versions" and "Unlist the version".

The publish and unlist jobs deliberately run no repository code, so the logic lives inline in the workflow. These
tests extract the part of each inline script above the "# --- main ---" marker and execute only that.
Run: python -I -m unittest discover -s .github/scripts -p "test_*.py".
"""
import textwrap
import unittest
from pathlib import Path

WORKFLOW = Path(__file__).resolve().parents[1] / "workflows" / "nuget-publish.yml"
STEP = "- name: Unlist older versions"
MANUAL_STEP = "- name: Unlist the version"
MARKER = "# --- main ---"


def load_selection(step: str = STEP) -> dict:
    lines = WORKFLOW.read_text(encoding="utf-8").splitlines()
    start = next(i for i, line in enumerate(lines) if line.strip() == step)
    run = next(i for i in range(start, len(lines)) if lines[i].strip() == "run: |")
    indent = len(lines[run]) - len(lines[run].lstrip()) + 2
    body = []
    for line in lines[run + 1:]:
        if line.strip() and len(line) - len(line.lstrip()) < indent:
            break
        body.append(line)
    script = textwrap.dedent("\n".join(body))
    if MARKER not in script:
        raise AssertionError(f"marker {MARKER!r} not found in the unlist step")
    namespace: dict = {}
    exec(script.split(MARKER)[0], namespace)  # noqa: S102 - our own workflow text, pure functions only
    return namespace


class UnlistSelectionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.to_unlist = staticmethod(load_selection()["to_unlist"])

    def test_new_major_keeps_previous_major_listed(self) -> None:
        self.assertEqual(self.to_unlist("4.0.0", ["3.6.0"]), [])

    def test_patch_unlists_older_versions_of_same_major_only(self) -> None:
        self.assertEqual(self.to_unlist("4.0.1", ["4.0.0", "3.6.0"]), ["4.0.0"])

    def test_patch_on_previous_major_leaves_newer_major_alone(self) -> None:
        self.assertEqual(self.to_unlist("3.6.1", ["4.0.0", "3.6.0"]), ["3.6.0"])

    def test_minor_unlists_all_lower_versions_of_same_major(self) -> None:
        self.assertEqual(sorted(self.to_unlist("3.6.0", ["3.5.0", "3.4.2", "2.9.0"])), ["3.4.2", "3.5.0"])

    def test_listed_prereleases_are_never_unlisted(self) -> None:
        self.assertEqual(self.to_unlist("4.0.1", ["4.0.0-rc.1", "4.0.0"]), ["4.0.0"])

    def test_prerelease_publish_unlists_nothing(self) -> None:
        self.assertEqual(self.to_unlist("4.1.0-rc.1", ["4.0.0", "4.0.1"]), [])

    def test_build_metadata_is_ignored(self) -> None:
        self.assertEqual(self.to_unlist("3.6.1", ["3.6.0+abc"]), ["3.6.0+abc"])

    def test_same_or_higher_version_is_not_unlisted(self) -> None:
        self.assertEqual(self.to_unlist("3.6.0", ["3.6.0", "3.7.0"]), [])


class ManualUnlistDecisionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.decide = staticmethod(load_selection(MANUAL_STEP)["decide"])

    def test_listed_version_is_deleted(self) -> None:
        self.assertEqual(self.decide("3.6.2", [("3.6.1", False), ("3.6.2", True)]), "delete")

    def test_already_unlisted_version_is_skipped(self) -> None:
        self.assertEqual(self.decide("3.6.2", [("3.6.2", False), ("4.1.0", True)]), "skip")

    def test_version_absent_from_registration_is_missing(self) -> None:
        self.assertEqual(self.decide("3.6.2", [("3.6.1", True), ("4.1.0", True)]), "missing")

    def test_no_versions_is_missing(self) -> None:
        self.assertEqual(self.decide("3.6.2", []), "missing")

    def test_comparison_ignores_case(self) -> None:
        self.assertEqual(self.decide("4.0.0-RC.1", [("4.0.0-rc.1", True)]), "delete")


if __name__ == "__main__":
    unittest.main()


class DeleteFailureTests(unittest.TestCase):
    """Both privileged steps carry their own copy (they run no repository code); both must behave the same."""

    def test_describe_failure_masks_the_key_and_keeps_the_exit_code(self) -> None:
        for step in (STEP, MANUAL_STEP):
            describe = load_selection(step)["describe_failure"]
            line = describe(1, "Response status code does not indicate success: 403 (key oy2abc is invalid)\n", "oy2abc")
            self.assertEqual(line, "exit 1: Response status code does not indicate success: 403 (key *** is invalid)")

    def test_describe_failure_keeps_only_the_tail_of_long_output(self) -> None:
        for step in (STEP, MANUAL_STEP):
            line = load_selection(step)["describe_failure"](2, "x" * 1000 + " end", "")
            self.assertTrue(line.endswith(" end"))
            self.assertLessEqual(len(line), len("exit 2: ") + 400)

    def test_permission_and_missing_errors_are_not_retried(self) -> None:
        for step in (STEP, MANUAL_STEP):
            is_permanent = load_selection(step)["is_permanent"]
            self.assertTrue(is_permanent("Response status code does not indicate success: 403 (Forbidden)."))
            self.assertTrue(is_permanent("Response status code does not indicate success: 401 (Unauthorized)."))
            self.assertTrue(is_permanent("Response status code does not indicate success: 404 (Not Found)."))
            self.assertFalse(is_permanent("Response status code does not indicate success: 503 (Service Unavailable)."))
            self.assertFalse(is_permanent("The request timed out after 4040 ms"))
