"""Removes CodeQL results located in build output (obj/, bin/) from SARIF files before they are uploaded.

C# is analyzed with a real build, which gives the most accurate results, but then the CodeQL config's paths-ignore has
no effect (GitHub applies it to compiled languages only without a build). The build also produces source-generator
output under obj/ (System.Text.Json contexts, the OpenAPI XML comment generator), and CodeQL reports on that code,
which is not ours to fix. This script drops those results; every result in first-party source is kept.

Usage: codeql_sarif_filter.py (run from the workspace; rewrites sarif-results/csharp.sarif)
"""
from __future__ import annotations

import json
import logging
import sys
from pathlib import Path

log = logging.getLogger("codeql-sarif-filter")

BUILD_OUTPUT_DIRS = frozenset({"obj", "bin"})
# Fixed, not an argument or a directory listing: the analyze step writes <language>.sarif to output: sarif-results, and
# the workflow analyzes C# only. No input picks the path that is written.
SARIF_FILE = Path("sarif-results") / "csharp.sarif"


def is_generated(uri: str) -> bool:
    """True when the path has an obj/ or bin/ directory segment."""
    path = uri.removeprefix("file://").replace("\\", "/")
    return any(segment in BUILD_OUTPUT_DIRS for segment in path.split("/")[:-1])


def _location_uri(result: dict) -> str | None:
    locations = result.get("locations") or []
    if not locations:
        return None
    return locations[0].get("physicalLocation", {}).get("artifactLocation", {}).get("uri")


def filter_sarif(sarif: dict) -> int:
    """Removes results in build output from every run, in place; returns how many were removed."""
    removed = 0
    for run in sarif.get("runs", []):
        results = run.get("results", [])
        kept = [r for r in results if not ((uri := _location_uri(r)) and is_generated(uri))]
        removed += len(results) - len(kept)
        run["results"] = kept
    return removed


def filter_file(path: Path) -> int:
    """Filters one SARIF file in place; fails when it does not exist (the analysis wrote nowhere else)."""
    if not path.is_file():
        sys.exit(f"no SARIF file at {path}")
    sarif = json.loads(path.read_text(encoding="utf-8"))
    removed = filter_sarif(sarif)
    path.write_text(json.dumps(sarif), encoding="utf-8")
    return removed


def main() -> int:
    logging.basicConfig(level=logging.INFO, format="%(message)s")
    removed = filter_file(SARIF_FILE)
    log.info("%s: removed %d result(s) in build output", SARIF_FILE, removed)
    return 0


if __name__ == "__main__":
    sys.exit(main())
