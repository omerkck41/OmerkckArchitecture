"""Tests for codeql_sarif_filter.py. Run: python -I -m unittest discover -s .github/scripts -p "test_*.py"."""
import json
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import codeql_sarif_filter as csf  # noqa: E402


def result(uri: str, rule: str = "cs/useless-cast-to-self") -> dict:
    return {"ruleId": rule, "locations": [{"physicalLocation": {"artifactLocation": {"uri": uri}}}]}


class IsGeneratedTests(unittest.TestCase):
    def test_build_output_paths_are_generated(self) -> None:
        for uri in (
            "src/Providers/Kck.Observability.OpenTelemetry/obj/Release/net10.0/generated/X/Health.g.cs",
            "samples/Kck.Sample.Orders/obj/Release/net10.0/generated/OpenApiXmlCommentSupport.generated.cs",
            "tests/Kck.Http.Resilience.Tests/bin/Release/net10.0/Foo.cs",
            "obj/Release/Foo.cs",
        ):
            self.assertTrue(csf.is_generated(uri), uri)

    def test_source_paths_are_kept(self) -> None:
        for uri in (
            "src/Providers/Kck.Persistence.EntityFramework/Repositories/EfRepository.cs",
            "src/Providers/Kck.Objects/Object.cs",  # "obj" only as part of a name
            "tests/Kck.Binary.Tests/BinTests.cs",
        ):
            self.assertFalse(csf.is_generated(uri), uri)

    def test_windows_separators_and_file_scheme_are_understood(self) -> None:
        self.assertTrue(csf.is_generated("file:///home/runner/work/repo/repo/src/A/obj/Release/B.g.cs"))
        self.assertTrue(csf.is_generated("src\\A\\obj\\Release\\B.g.cs"))


class FilterTests(unittest.TestCase):
    def test_results_in_build_output_are_removed_and_others_kept(self) -> None:
        sarif = {"runs": [{"results": [
            result("src/A/obj/Release/net10.0/generated/X.g.cs"),
            result("src/A/Repositories/EfRepository.cs", "cs/missed-ternary-operator"),
            result("tests/B/bin/Release/Y.cs"),
        ]}]}

        removed = csf.filter_sarif(sarif)

        self.assertEqual(removed, 2)
        self.assertEqual([r["ruleId"] for r in sarif["runs"][0]["results"]], ["cs/missed-ternary-operator"])

    def test_location_given_by_artifact_index_is_resolved(self) -> None:
        # SARIF lets a location name its file by an index into run.artifacts instead of a uri.
        by_index = {"ruleId": "cs/x", "locations": [{"physicalLocation": {"artifactLocation": {"index": 1}}}]}
        sarif = {"runs": [{
            "artifacts": [{"location": {"uri": "src/A/B.cs"}}, {"location": {"uri": "src/A/obj/X.g.cs"}}],
            "results": [by_index, result("src/A/B.cs")],
        }]}

        self.assertEqual(csf.filter_sarif(sarif), 1)
        self.assertEqual(len(sarif["runs"][0]["results"]), 1)

    def test_result_without_location_is_kept(self) -> None:
        sarif = {"runs": [{"results": [{"ruleId": "cs/x"}]}]}

        self.assertEqual(csf.filter_sarif(sarif), 0)
        self.assertEqual(len(sarif["runs"][0]["results"]), 1)

    def test_file_is_rewritten(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "csharp.sarif"
            path.write_text(json.dumps({"runs": [{"results": [result("src/A/obj/X.g.cs"), result("src/A/B.cs")]}]}),
                            encoding="utf-8")

            removed = csf.filter_file(path)

            self.assertEqual(removed, 1)
            kept = json.loads(path.read_text(encoding="utf-8"))["runs"][0]["results"]
            self.assertEqual([r["locations"][0]["physicalLocation"]["artifactLocation"]["uri"] for r in kept],
                             ["src/A/B.cs"])

    def test_missing_file_fails(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            with self.assertRaises(SystemExit):
                csf.filter_file(Path(tmp) / "csharp.sarif")

    def test_workflow_analyzes_csharp_only(self) -> None:
        # The script rewrites the fixed csharp.sarif; another language would be uploaded unfiltered.
        workflow = (Path(__file__).resolve().parents[1] / "workflows" / "codeql.yml").read_text(encoding="utf-8")

        self.assertIn("language: [csharp]", workflow)
        self.assertEqual(csf.SARIF_FILE, Path("sarif-results") / "csharp.sarif")


if __name__ == "__main__":
    unittest.main()
