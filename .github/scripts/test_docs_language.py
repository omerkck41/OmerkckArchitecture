"""Tests for docs_language.py. Run: python -I -m unittest discover -s .github/scripts -p "test_*.py"."""
import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import docs_language as dl  # noqa: E402


class FindTurkishTests(unittest.TestCase):
    def test_english_prose_has_no_findings(self):
        text = "# Persistence\n\nSoft-deleted rows are hidden from every query.\n"

        self.assertEqual(dl.find_turkish(text), [])

    def test_turkish_letters_are_reported_with_line_number(self):
        text = "# Title\n\nKayıtlar silinmez.\n"

        findings = dl.find_turkish(text)

        self.assertEqual([f.line for f in findings], [3])

    def test_ascii_turkish_words_are_reported(self):
        text = "Repository'ler `IUnitOfWork` uzerinden alinir ve kaydedilir.\n"

        self.assertEqual(len(dl.find_turkish(text)), 1)

    def test_code_blocks_and_inline_code_are_ignored(self):
        text = "Use `ve` here.\n\n```csharp\nvar kayıt = ile;\n```\n\n~~~\nbir değil\n~~~\n"

        self.assertEqual(dl.find_turkish(text), [])

    def test_names_with_turkish_letters_can_be_allowed(self):
        text = "Maintainer: Ömer KÜÇÜK\n"

        self.assertEqual(dl.find_turkish(text, allowed_words={"Ömer", "KÜÇÜK"}), [])

    def test_html_comments_are_ignored(self):
        text = "<!-- bu bir not -->\nEnglish text.\n"

        self.assertEqual(dl.find_turkish(text), [])


class CheckFilesTests(unittest.TestCase):
    def test_listed_file_with_turkish_fails_and_missing_file_fails(self):
        with tempfile.TemporaryDirectory() as root:
            Path(root, "a.md").write_text("Bu dosya Türkçe.\n", encoding="utf-8")
            Path(root, "b.md").write_text("This one is English.\n", encoding="utf-8")

            problems = dl.check_files(Path(root), ["a.md", "b.md", "missing.md"])

        self.assertEqual(sorted(p.split(":")[0] for p in problems), ["a.md", "missing.md"])

    def test_entries_outside_the_root_are_rejected(self):
        with tempfile.TemporaryDirectory() as root:
            Path(root, "inner").mkdir()

            problems = dl.check_files(Path(root, "inner"), ["../outside.md"])

        self.assertEqual(len(problems), 1)
        self.assertIn("outside the repository", problems[0])

    def test_list_file_outside_the_root_is_rejected(self):
        with tempfile.TemporaryDirectory() as root, tempfile.TemporaryDirectory() as other:
            listing = Path(other, "english.txt")
            listing.write_text("a.md\n", encoding="utf-8")

            with self.assertRaises(ValueError):
                dl.inside(Path(root), listing)

    def test_published_pages_missing_from_the_list_are_reported(self):
        with tempfile.TemporaryDirectory() as root:
            for name in ["README.md", "docs/a.md", "docs/plans/p.md", "docs/adr/0003-old.md", "docs/adr/0031-new.md"]:
                Path(root, name).parent.mkdir(parents=True, exist_ok=True)
                Path(root, name).write_text("English.\n", encoding="utf-8")

            missing = dl.unlisted_published_pages(Path(root), ["README.md"])

        self.assertEqual(missing, ["docs/a.md", "docs/adr/0031-new.md"])

    def test_read_list_skips_comments_and_blank_lines(self):
        with tempfile.TemporaryDirectory() as root:
            listing = Path(root, "english.txt")
            listing.write_text("# comment\n\ndocs/a.md\n  docs/b.md  \n", encoding="utf-8")

            self.assertEqual(dl.read_list(listing), ["docs/a.md", "docs/b.md"])


if __name__ == "__main__":
    unittest.main()
