"""Tests for publicapi_ship.py. Run: python -I -m unittest discover -s .github/scripts -p "test_*.py"."""
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import publicapi_ship as ps  # noqa: E402

BOM = "\N{ZERO WIDTH NO-BREAK SPACE}"  # U+FEFF, written as a named escape so the source has no invisible character


class PublicApiShipTests(unittest.TestCase):
    def setUp(self) -> None:
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self) -> None:
        self._tmp.cleanup()

    def project(self, name: str, shipped: str, unshipped: str) -> Path:
        directory = self.root / "src" / "Abstractions" / name
        directory.mkdir(parents=True)
        (directory / ps.SHIPPED).write_bytes(shipped.encode("utf-8"))
        (directory / ps.UNSHIPPED).write_bytes(unshipped.encode("utf-8"))
        return directory

    def test_ship_added_entries_appended_in_unshipped_order_and_unshipped_emptied(self) -> None:
        directory = self.project("Kck.A", "#nullable enable\nA.X\nA.Z\n", "#nullable enable\nA.Y\nA.B\n")

        results = ps.ship(self.root)

        self.assertEqual([(r.project, r.removed, r.added) for r in results], [("Kck.A", 0, 2)])
        self.assertEqual((directory / ps.SHIPPED).read_bytes(), b"#nullable enable\nA.X\nA.Z\nA.Y\nA.B\n")
        self.assertEqual((directory / ps.UNSHIPPED).read_bytes(), b"#nullable enable\n")

    def test_ship_removed_entry_deletes_exact_shipped_line(self) -> None:
        directory = self.project(
            "Kck.A",
            "#nullable enable\nA.M(int x) -> void\nA.M(int x, int y) -> void\n",
            "#nullable enable\n*REMOVED*A.M(int x) -> void\nA.N() -> void\n",
        )

        results = ps.ship(self.root)

        self.assertEqual((results[0].removed, results[0].added), (1, 1))
        self.assertEqual(
            (directory / ps.SHIPPED).read_bytes(),
            b"#nullable enable\nA.M(int x, int y) -> void\nA.N() -> void\n",
        )

    def test_ship_removed_entry_missing_from_shipped_raises_and_writes_nothing(self) -> None:
        good = self.project("Kck.A", "#nullable enable\nA.X\n", "#nullable enable\nA.Y\n")
        bad = self.project("Kck.B", "#nullable enable\nB.X\n", "#nullable enable\n*REMOVED*B.Gone\n")

        with self.assertRaisesRegex(ps.ShipError, "Kck.B: removed entry not found"):
            ps.ship(self.root)

        self.assertEqual((good / ps.SHIPPED).read_bytes(), b"#nullable enable\nA.X\n")
        self.assertEqual((good / ps.UNSHIPPED).read_bytes(), b"#nullable enable\nA.Y\n")
        self.assertEqual((bad / ps.UNSHIPPED).read_bytes(), b"#nullable enable\n*REMOVED*B.Gone\n")

    def test_ship_entry_already_shipped_raises_duplicate(self) -> None:
        self.project("Kck.A", "#nullable enable\nA.X\n", "#nullable enable\nA.X\n")

        with self.assertRaisesRegex(ps.ShipError, "already in"):
            ps.ship(self.root)

    def test_ship_crlf_and_bom_input_written_as_lf_without_bom(self) -> None:
        directory = self.project("Kck.A", BOM + "#nullable enable\r\nA.X\r\n", BOM + "#nullable enable\r\nA.Y\r\n")

        ps.ship(self.root)

        self.assertEqual((directory / ps.SHIPPED).read_bytes(), b"#nullable enable\nA.X\nA.Y\n")
        self.assertEqual((directory / ps.UNSHIPPED).read_bytes(), b"#nullable enable\n")

    def test_ship_unshipped_lines_before_header_keep_header_first(self) -> None:
        directory = self.project("Kck.A", "#nullable enable\nA.X\n", "A.Y\n#nullable enable\nA.Z\n")

        ps.ship(self.root)

        self.assertEqual((directory / ps.SHIPPED).read_bytes(), b"#nullable enable\nA.X\nA.Y\nA.Z\n")
        self.assertEqual((directory / ps.UNSHIPPED).read_bytes(), b"#nullable enable\n")

    def test_main_invalid_root_exits_2(self) -> None:
        with self.assertLogs("publicapi-ship", level="ERROR"):
            self.assertEqual(ps.main(["--root", str(self.root / "missing")]), 2)

    def test_find_unshipped_ignores_build_output_copies(self) -> None:
        self.project("Kck.A", "#nullable enable\n", "#nullable enable\n")
        stray = self.root / "src" / "Abstractions" / "Kck.A" / "obj"
        stray.mkdir()
        (stray / ps.UNSHIPPED).write_bytes(b"#nullable enable\nX\n")

        self.assertEqual([p.parent.name for p in ps.find_unshipped(self.root)], ["Kck.A"])

    def test_check_pending_entries_exits_1(self) -> None:
        self.project("Kck.A", "#nullable enable\n", "#nullable enable\n")
        self.project("Kck.B", "#nullable enable\nB.X\n", "#nullable enable\n*REMOVED*B.X\nB.Y\n")

        self.assertEqual(ps.check(self.root), [("Kck.B", 2)])
        with self.assertLogs("publicapi-ship", level="ERROR"):
            self.assertEqual(ps.main(["--root", str(self.root), "--check"]), 1)

    def test_check_all_empty_exits_0_and_writes_nothing(self) -> None:
        directory = self.project("Kck.A", "#nullable enable\nA.X\n", "#nullable enable\n\n")

        with self.assertLogs("publicapi-ship", level="INFO"):
            self.assertEqual(ps.main(["--root", str(self.root), "--check"]), 0)
        self.assertEqual((directory / ps.UNSHIPPED).read_bytes(), b"#nullable enable\n\n")


if __name__ == "__main__":
    unittest.main()
