"""Fails when a published documentation file that must be English contains Turkish text (ADR-0029 D2/D5, plan G2.1).

The files to check are listed in .github/docs-english.txt; files are added as they are translated, and at the end of
plan phase F2 the list covers every published page. Code blocks, inline code and HTML comments are ignored. A line is
reported when it has a letter used only in Turkish or a common Turkish word (also written without diacritics).

Usage: docs_language.py [--list .github/docs-english.txt] [--root .]
"""
from __future__ import annotations

import argparse
import logging
import re
import sys
from dataclasses import dataclass
from pathlib import Path

log = logging.getLogger("docs-language")

REPO = Path(__file__).resolve().parents[2]
DEFAULT_LIST = REPO / ".github" / "docs-english.txt"

TURKISH_LETTERS = re.compile(r"[çÇğĞıİöÖşŞüÜ]")
# Words that exist only in Turkish (with and without diacritics); English homographs (her, once, gore, ama) are left out.
TURKISH_WORDS = frozenset({
    "ve", "ile", "icin", "için", "bir", "bu", "şu", "olarak", "yalniz", "yalnız", "degil", "değil", "gibi", "veya",
    "ancak", "daha", "cok", "çok", "olan", "eder", "edilir", "edilen", "kullanin", "kullanın", "kullanilir",
    "kullanılır", "icinde", "içinde", "sonra", "önce", "göre", "hic", "hiç", "kayit", "kayıt", "olur", "oldugu",
    "olduğu", "uzerinden", "üzerinden", "ise", "nasil", "nasıl", "ornek", "örnek", "ayni", "aynı", "yoksa", "varsa",
    # Frequent in the current Turkish docs (headings, tables).
    "paket", "paketler", "hedef", "ozellik", "özellik", "kullanim", "kullanım", "ornekler", "örnekler", "notlar",
    "secim", "seçim", "kriterleri", "ayarlar", "yapilandirma", "yapılandırma", "baglanti", "bağlantı", "varsayilan",
    "varsayılan", "gerekir", "gerekli", "zorunlu", "istege", "isteğe", "bagli", "bağlı", "desteklenir", "desteklenmez",
    "sinir", "sınır", "dikkat", "uyari", "uyarı", "neden", "nedir", "doner", "döner", "icin", "kurulum", "kaynak",
    "kaynaklar", "sorun", "sorunlar", "cozum", "çözüm", "aciklama", "açıklama", "tum", "tüm", "yeni", "eski",
})
WORD = re.compile(r"[A-Za-zçÇğĞıİöÖşŞüÜ]+")
INLINE_CODE = re.compile(r"`[^`]*`")
HTML_COMMENT = re.compile(r"<!--.*?-->", re.DOTALL)
FENCE = re.compile(r"^\s*(```|~~~)")


@dataclass(frozen=True)
class Finding:
    line: int
    text: str


def find_turkish(text: str, allowed_words: frozenset[str] | set[str] = frozenset()) -> list[Finding]:
    """Returns the prose lines of a Markdown text that contain Turkish."""
    findings: list[Finding] = []
    text = HTML_COMMENT.sub(lambda m: "\n" * m.group(0).count("\n"), text)
    in_code = False
    for number, line in enumerate(text.splitlines(), start=1):
        if FENCE.match(line):
            in_code = not in_code
            continue
        if in_code:
            continue
        prose = INLINE_CODE.sub(" ", line)
        words = [w for w in WORD.findall(prose) if w not in allowed_words]
        if any(TURKISH_LETTERS.search(w) for w in words) or any(w.lower() in TURKISH_WORDS for w in words):
            findings.append(Finding(number, line.strip()))
    return findings


def inside(root: Path, path: Path) -> Path:
    """Returns path resolved, or raises ValueError when it is outside root (paths come from the command line)."""
    resolved = path.resolve()
    if not resolved.is_relative_to(root.resolve()):
        raise ValueError(f"{path} is outside the repository root {root}")
    return resolved


def read_list(path: Path) -> list[str]:
    """Reads the repository-relative paths of the files that must be English."""
    entries = (line.strip() for line in path.read_text(encoding="utf-8").splitlines())
    return [entry for entry in entries if entry and not entry.startswith("#")]


def check_files(root: Path, files: list[str], allowed_words: frozenset[str] | set[str] = frozenset()) -> list[str]:
    """Returns one problem per Turkish line or missing file, as 'path:line: text'."""
    problems: list[str] = []
    for relative in files:
        try:
            path = inside(root, root / relative)
        except ValueError:
            problems.append(f"{relative}: outside the repository root")
            continue
        if not path.is_file():
            problems.append(f"{relative}: listed in the English documentation list but not found")
            continue
        for finding in find_turkish(path.read_text(encoding="utf-8"), allowed_words):
            problems.append(f"{relative}:{finding.line}: {finding.text}")
    return problems


# Proper names that contain Turkish letters.
ALLOWED_WORDS = frozenset({"Ömer", "KÜÇÜK", "Küçük"})


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--list", type=Path, default=DEFAULT_LIST)
    parser.add_argument("--root", type=Path, default=REPO)
    args = parser.parse_args(argv)
    sys.stdout.reconfigure(encoding="utf-8")  # Windows consoles default to a code page that cannot print every line
    logging.basicConfig(level=logging.INFO, format="%(message)s", stream=sys.stdout)

    root = args.root.resolve()
    files = read_list(inside(root, args.list))
    problems = check_files(root, files, ALLOWED_WORDS)
    for problem in problems:
        path, _, rest = problem.partition(":")
        line, _, text = rest.partition(":")
        if line.strip().isdigit():
            log.error("::error file=%s,line=%s::Turkish text in English documentation:%s", path, line.strip(), text)
        else:
            log.error("::error file=%s::%s", path, rest.strip())
    log.info("docs language: %d file(s) checked, %d problem(s)", len(files), len(problems))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
