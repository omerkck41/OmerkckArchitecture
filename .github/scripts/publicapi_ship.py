"""Moves the pending public API of every abstraction project from PublicAPI.Unshipped.txt to PublicAPI.Shipped.txt.

Release step (docs/policies/versioning.md, "Shipping the public API"). For each PublicAPI.Unshipped.txt under
src/Abstractions:
- a `*REMOVED*<signature>` line deletes exactly `<signature>` from the sibling PublicAPI.Shipped.txt (an error when
  Shipped does not contain it);
- every other non-empty line except `#nullable enable` is appended to Shipped in Unshipped order (an error when
  Shipped already contains it). Shipped is not re-sorted; its `#nullable enable` header stays first;
- Unshipped is rewritten to the single line `#nullable enable`.
Files are written as UTF-8 without BOM with LF line endings. No file is written when a project's REMOVED or duplicate
lines cannot be resolved; an I/O error during the write loop can leave earlier projects written — re-run after fixing.

Usage:
  publicapi_ship.py [--root <repo>]           ship every project, print removed/added per project
  publicapi_ship.py [--root <repo>] --check   exit 1 when any PublicAPI.Unshipped.txt still has entries
"""
from __future__ import annotations

import argparse
import logging
import sys
from dataclasses import dataclass
from pathlib import Path

log = logging.getLogger("publicapi-ship")

NULLABLE_HEADER = "#nullable enable"
REMOVED_PREFIX = "*REMOVED*"
UNSHIPPED = "PublicAPI.Unshipped.txt"
SHIPPED = "PublicAPI.Shipped.txt"
ABSTRACTIONS_DIR = Path("src") / "Abstractions"
BUILD_OUTPUT_DIRS = frozenset({"bin", "obj"})


class ShipError(Exception):
    """A pending entry cannot be applied to PublicAPI.Shipped.txt."""


@dataclass(frozen=True)
class ShipResult:
    project: str
    removed: int
    added: int
    shipped_text: str


def _read_lines(path: Path) -> list[str]:
    # utf-8-sig tolerates a BOM on input; splitlines() accepts CRLF and LF alike.
    return path.read_text(encoding="utf-8-sig").splitlines()


def _write_lines(path: Path, lines: list[str]) -> None:
    path.write_bytes(("\n".join(lines) + "\n").encode("utf-8"))


def pending_entries(unshipped: Path) -> list[str]:
    """The Unshipped lines that still have to ship: everything except blank lines and the nullable header."""
    return [
        line.strip()
        for line in _read_lines(unshipped)
        if line.strip() and line.strip() != NULLABLE_HEADER
    ]


def find_unshipped(root: Path) -> list[Path]:
    """Every PublicAPI.Unshipped.txt under src/Abstractions, skipping build output, in a stable order."""
    base = root / ABSTRACTIONS_DIR
    return sorted(
        path
        for path in base.rglob(UNSHIPPED)
        if not BUILD_OUTPUT_DIRS.intersection(path.relative_to(base).parts[:-1])
    )


def ship_project(unshipped: Path) -> ShipResult:
    """Computes the new Shipped content for one project without writing anything."""
    shipped = unshipped.with_name(SHIPPED)
    project = unshipped.parent.name
    if not shipped.is_file():
        raise ShipError(f"{project}: {SHIPPED} is missing")

    lines = _read_lines(shipped)
    removed = added = 0
    entries = pending_entries(unshipped)

    for entry in entries:
        if not entry.startswith(REMOVED_PREFIX):
            continue
        signature = entry[len(REMOVED_PREFIX):]
        try:
            lines.remove(signature)
        except ValueError:
            raise ShipError(f"{project}: removed entry not found in {SHIPPED}: {signature}") from None
        removed += 1

    present = set(lines)
    for entry in entries:
        if entry.startswith(REMOVED_PREFIX):
            continue
        if entry in present:
            raise ShipError(f"{project}: entry already in {SHIPPED}: {entry}")
        lines.append(entry)
        present.add(entry)
        added += 1

    if NULLABLE_HEADER in lines and lines[0] != NULLABLE_HEADER:
        lines.remove(NULLABLE_HEADER)
        lines.insert(0, NULLABLE_HEADER)

    return ShipResult(project, removed, added, "\n".join(lines) + "\n")


def ship(root: Path) -> list[ShipResult]:
    """Ships every project; writes only after every project's entries were resolved (an I/O error mid-loop is not
    rolled back)."""
    unshipped_files = find_unshipped(root)
    results = [ship_project(path) for path in unshipped_files]
    for path, result in zip(unshipped_files, results):
        path.with_name(SHIPPED).write_bytes(result.shipped_text.encode("utf-8"))
        _write_lines(path, [NULLABLE_HEADER])
    return results


def check(root: Path) -> list[tuple[str, int]]:
    """Projects whose Unshipped file still has entries, with the entry count."""
    return [
        (path.parent.name, count)
        for path in find_unshipped(root)
        if (count := len(pending_entries(path)))
    ]


def main(argv: list[str] | None = None) -> int:
    logging.basicConfig(level=logging.INFO, format="%(message)s", stream=sys.stdout)
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--root", type=Path, default=Path.cwd(), help="repository root (default: current directory)")
    parser.add_argument("--check", action="store_true", help="exit 1 when any Unshipped file has entries")
    args = parser.parse_args(argv)
    root: Path = args.root.resolve()

    if not (root / ABSTRACTIONS_DIR).is_dir():
        log.error("%s not found under %s", ABSTRACTIONS_DIR.as_posix(), root)
        return 2

    if args.check:
        pending = check(root)
        for project, count in pending:
            log.error("%s: %d unshipped entries", project, count)
        if pending:
            log.error("run .github/scripts/publicapi_ship.py before the release")
            return 1
        log.info("all PublicAPI.Unshipped.txt files are empty")
        return 0

    try:
        results = ship(root)
    except ShipError:
        log.exception("public API ship aborted (nothing was written)")
        return 1
    for result in results:
        log.info("%s: removed %d, added %d", result.project, result.removed, result.added)
    log.info(
        "%d projects: removed %d, added %d",
        len(results),
        sum(r.removed for r in results),
        sum(r.added for r in results),
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
