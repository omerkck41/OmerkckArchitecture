"""Turns the C# code blocks of selected README files and the docs/ guides into compilable sources (F3 H21, G2.1 b).

Every ```csharp block of the files in README_FILES and of the published guides under docs/ becomes
samples/Kck.ReadmeSnippets/Generated/<Name>.g.cs in its own namespace: leading `using` lines stay usings (inside the
namespace), statements are wrapped in a method, and type declarations stay at namespace level. `#line` directives point
compiler errors at the source line. Names a block uses without defining (the reader's own types) come from
hand-written files in samples/Kck.ReadmeSnippets/Context.

Usage: readme_snippets.py            regenerate the files
       readme_snippets.py --check    exit 1 when a generated file is missing, stale or orphaned (source edited only)
"""
from __future__ import annotations

import argparse
import logging
import re
import sys
from dataclasses import dataclass
from pathlib import Path

log = logging.getLogger("readme-snippets")

REPO = Path(__file__).resolve().parents[2]
OUTPUT = REPO / "samples" / "Kck.ReadmeSnippets" / "Generated"

# Bundles and the most downloaded packages first (roadmap H21).
README_FILES = [
    "README.md",
    "src/Bundles/Kck.Bundle.WebApi/README.nuget.md",
    "src/Bundles/Kck.Bundle.MinimalApi/README.nuget.md",
    "src/Bundles/Kck.Bundle.WorkerService/README.nuget.md",
    "src/Abstractions/Kck.Persistence.Abstractions/README.nuget.md",
    "src/Providers/Kck.Persistence.EntityFramework/README.nuget.md",
    "src/Providers/Kck.Pipeline.Mediator/README.nuget.md",
    "src/Providers/Kck.Caching.InMemory/README.nuget.md",
    "src/Providers/Kck.EventBus.InMemory/README.nuget.md",
    "src/Providers/Kck.EventBus.RabbitMq/README.nuget.md",
]

# Published guides (plan G2.1 b). ADRs are immutable records of the API at decision time and are not compiled.
DOCS_ROOT = "docs"
DOCS_EXCLUDED = ("docs/adr/", "docs/plans/", "docs/archive/", "docs/memory/", "docs/migrations/_template.md")
# A block preceded by this comment is not compiled (an excerpt, or code that is wrong on purpose); the text after
# "skip" says why. The count may only go down (plan R6): lower MAX_SKIPPED when a skipped block becomes compilable.
SKIP = re.compile(r"^<!--\s*snippet:\s*skip\b.*-->$")
MAX_SKIPPED = 4
CSHARP_FENCE = re.compile(r"^```\s*(?:csharp|cs|c#)(?:\s|\{|$)", re.IGNORECASE)

DECLARATION = re.compile(
    r"^(?:\[.*\]\s*)?(?:(?:public|internal|file|sealed|static|abstract|partial|readonly)\s+)*"
    r"(?:class|record|interface|struct|enum)\b")
USING = re.compile(r"^using\s+[\w.]+(?:\s*=\s*[\w.<>]+)?\s*;\s*$")
# Hosting variables a block may use without declaring them (it continues an earlier Program.cs).
AMBIENT = [("args", "string[]"), ("builder", "WebApplicationBuilder"), ("app", "WebApplication"),
           ("services", "IServiceCollection"), ("configuration", "IConfiguration"),
           ("connectionString", "string"), ("ct", "CancellationToken")]


@dataclass(frozen=True)
class Block:
    readme: str
    index: int
    first_line: int  # 1-based README line of the block's first code line
    lines: list[str]
    skipped: bool = False

    @property
    def name(self) -> str:
        path = Path(self.readme)
        if path.parts[0] == DOCS_ROOT:
            # docs/providers/http.md -> DocsProvidersHttp; the folder alone would collide (providers_1).
            words = re.split(r"[^A-Za-z0-9]+", path.with_suffix("").as_posix())
            stem = "".join(word[:1].upper() + word[1:] for word in words if word)
        elif path.parent == Path("."):
            stem = "Readme"
        else:
            stem = path.parent.name.removeprefix("Kck.").replace(".", "")
        return f"{stem}_{self.index}"


def extract_blocks(readme: str, text: str) -> list[Block]:
    blocks: list[Block] = []
    current: list[str] | None = None
    start = 0
    skip = False
    previous = ""
    for number, line in enumerate(text.splitlines(), start=1):
        stripped = line.strip()
        if current is None and CSHARP_FENCE.match(stripped) and stripped != "```csharp":
            # Any other C# fence spelling would silently escape compilation (and the skip ratchet).
            raise ValueError(f"{readme}:{number}: write C# code fences as ```csharp (found {stripped!r})")
        if current is None and stripped == "```csharp":
            current, start, skip = [], number + 1, bool(SKIP.match(previous))
        elif current is not None and stripped == "```":
            blocks.append(Block(readme, len(blocks) + 1, start, current, skip))
            current = None
        elif current is not None:
            current.append(line)
        if stripped:
            previous = stripped
    if current is not None:
        raise ValueError(f"{readme}: unterminated csharp block starting at line {start - 1}")
    return blocks


def render(block: Block) -> str:
    usings: list[str] = []
    body_start = 0
    for offset, line in enumerate(block.lines):
        if USING.match(line):
            usings.append(line.strip())
            body_start = offset + 1
        elif line.strip() and not line.lstrip().startswith("//"):
            break

    segments = split_segments(block.lines[body_start:])
    source = "\n".join(block.lines)
    readme_path = block.readme.replace("\\", "/")

    def with_line(offset: int, lines: list[str]) -> list[str]:
        return [f'#line {block.first_line + body_start + offset} "{readme_path}"', *lines, "#line default"]

    # The block's usings go inside the namespace so they win over the project's global usings, as a reader's own
    # usings would (Mediator.IQuery<T> vs Kck.Persistence.Abstractions.Repositories.IQuery<T>).
    out = ["// <auto-generated> by .github/scripts/readme_snippets.py - edit the README, then regenerate.",
           "#nullable enable", "", f"namespace ReadmeSnippets.{block.name};", *usings, ""]
    statements = [s for s in segments if not s.declaration and any(line.strip() for line in s.lines)]
    if statements:
        parameters = ", ".join(f"{clr} {name}" for name, clr in AMBIENT
                               if re.search(rf"\b{name}\b", source)
                               and not re.search(rf"\bvar\s+{name}\b", source))
        out += ["internal static class Snippet", "{",
                f"    internal static async Task RunAsync({parameters})", "    {"]
        for segment in statements:
            out += with_line(segment.offset, segment.lines)
        out += ["        await Task.CompletedTask;", "    }", "}", ""]
    for segment in (s for s in segments if s.declaration):
        out += [*with_line(segment.offset, segment.lines), ""]
    return "\n".join(out)


@dataclass(frozen=True)
class Segment:
    offset: int  # index of the first line in the block body
    lines: list[str]
    declaration: bool


# A string literal, a char literal, or a line comment (`.` stops at the line end).
STRING_OR_COMMENT = re.compile(r'"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])\'|//.*')


@dataclass
class _DeclarationEnd:
    """Tracks the braces of one type declaration to find the line where it ends."""
    depth: int = 0
    braced: bool = False

    def ends_at(self, code: str) -> bool:
        self.depth += code.count("{") - code.count("}")
        self.braced = self.braced or "{" in code
        return self.depth <= 0 and ((self.braced and "}" in code) or code.endswith(";"))


def split_segments(body: list[str]) -> list[Segment]:
    """Splits a block body into runs of top-level type declarations and of statements, in order.

    Guides often declare the reader's types first and register them afterwards, so a declaration ends where its
    braces close (or at the ';' of a one-line record) and the statements after it start a new run. Blank and comment
    lines stay with the run they follow.
    """
    segments: list[Segment] = []
    start, declaration = 0, False
    current: _DeclarationEnd | None = None  # set while inside a declaration
    for index, line in enumerate(body):
        code = STRING_OR_COMMENT.sub("", line).strip()
        if current is None and code:
            starts = _starts_declaration(body, index, code)
            if starts != declaration:
                _close_segment(segments, body, start, index, declaration)
                start, declaration = index, starts
            current = _DeclarationEnd() if starts else None
        if current is not None and current.ends_at(code):
            current = None
    _close_segment(segments, body, start, len(body), declaration)
    return segments


def _close_segment(segments: list[Segment], body: list[str], start: int, end: int, declaration: bool) -> None:
    """Appends body[start:end] as a segment unless it is empty."""
    if end > start:
        segments.append(Segment(start, body[start:end], declaration))


def _starts_declaration(body: list[str], index: int, code: str) -> bool:
    """True when the line at index starts a type declaration, or an attribute that belongs to one."""
    return bool(DECLARATION.match(body[index])) or (code.startswith("[") and _next_is_declaration(body, index))


def _next_is_declaration(body: list[str], index: int) -> bool:
    """True when the attribute starting at index (possibly spanning lines) belongs to a type declaration below it."""
    depth = 0
    for line in body[index:]:
        code = STRING_OR_COMMENT.sub("", line).strip()
        if depth == 0 and code and not code.startswith("["):
            return bool(DECLARATION.match(line))
        depth += code.count("[") - code.count("]")
    return False


def source_files(root: Path = REPO) -> list[str]:
    """The README files plus every published guide under docs/ (repository-relative, POSIX)."""
    guides = sorted(p.relative_to(root).as_posix() for p in (root / DOCS_ROOT).rglob("*.md"))
    return README_FILES + [g for g in guides if not g.startswith(DOCS_EXCLUDED)]


def all_blocks(root: Path = REPO) -> list[Block]:
    blocks: list[Block] = []
    for readme in source_files(root):
        blocks += extract_blocks(readme, (root / readme).read_text(encoding="utf-8"))
    return blocks


def expected_files(blocks: list[Block]) -> dict[Path, str]:
    files: dict[Path, str] = {}
    for block in (b for b in blocks if not b.skipped):
        path = OUTPUT / f"{block.name}.g.cs"
        if path in files:
            # docs/a-b.md and docs/a_b.md would both be DocsAB_n: one block would silently never compile.
            raise ValueError(f"{block.readme}: generated name {block.name} is already used by another file; rename it")
        files[path] = render(block)
    return files


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--check", action="store_true", help="verify instead of writing")
    args = parser.parse_args(argv)
    logging.basicConfig(level=logging.INFO, format="%(message)s")

    blocks = all_blocks()
    expected = expected_files(blocks)
    existing = set(OUTPUT.glob("*.g.cs")) if OUTPUT.exists() else set()
    skipped = [b for b in blocks if b.skipped]
    for block in skipped:
        log.info("skipped: %s:%d", block.readme, block.first_line - 1)
    if len(skipped) != MAX_SKIPPED:
        # Equality, not "at most": when a skipped block becomes compilable, MAX_SKIPPED must go down with it, so the
        # freed slot cannot be reused later.
        log.error("%d code blocks are marked 'snippet: skip' but MAX_SKIPPED is %d: make a new block compile instead, "
                  "or lower MAX_SKIPPED when a skip was removed", len(skipped), MAX_SKIPPED)
        return 1

    if args.check:
        stale = [p for p, text in expected.items() if not p.exists() or p.read_text(encoding="utf-8") != text]
        orphaned = existing - expected.keys()
        for path in sorted(stale):
            log.error("stale: %s", path.relative_to(REPO).as_posix())
        for path in sorted(orphaned):
            log.error("orphaned: %s", path.relative_to(REPO).as_posix())
        if stale or orphaned:
            log.error("README or docs code blocks changed: run python .github/scripts/readme_snippets.py and commit the result")
            return 1
        log.info("%d README and docs code blocks up to date", len(expected))
        return 0

    OUTPUT.mkdir(parents=True, exist_ok=True)
    for path in existing - expected.keys():
        path.unlink()
    for path, text in expected.items():
        path.write_text(text, encoding="utf-8", newline="\n")
    log.info("wrote %d files to %s", len(expected), OUTPUT.relative_to(REPO).as_posix())
    return 0


if __name__ == "__main__":
    sys.exit(main())
