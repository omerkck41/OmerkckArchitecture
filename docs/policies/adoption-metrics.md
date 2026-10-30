# Adoption metrics

How we track whether the developer-experience work ([ADR-0029](../adr/0029-dx-first-adoption.md)) changes adoption.
These numbers are observed, not gated: they lag, and much of the NuGet traffic is automated (mirrors, scanners, CI
caches), so the trend matters more than the absolute value.

## How the numbers are taken

| Metric | Source | Note |
|---|---|---|
| Total downloads per package | nuget.org search API, `q=packageid:<id>`, field `totalDownloads` (all versions) | Monthly downloads = difference between two dated snapshots |
| Stars, forks, watchers | GitHub REST API `repos/omerkck41/OmerkckArchitecture` | |
| Issues opened by people outside the project | GitHub search: `type:issue -author:omerkck41 -author:app/dependabot` | The best single signal of real use |
| Docs and template feedback | Issues from the "Docs feedback" (label `docs`) and "Template problem" (title `template:`) forms, and the "Report a problem with the docs" link on every docs page | Counted within external issues; read each one |

Snapshots are taken at the baseline, 30 days after 4.6.0 (template) and 30 days after 4.7.0 (analyzers), and added
below as new rows; earlier rows are never edited. Next: **2026-11-29** (4.6.0, released 2026-10-30, + 30 days); then
30 days after 4.7.0 is published.

## Snapshots

| Date | Packages | Total downloads (all packages) | Median per package | `Kck.Templates` | `Kck.Persistence.EntityFramework` | `Kck.Bundle.WebApi` | Stars | External issues |
|---|---|---|---|---|---|---|---|---|
| 2026-10-26 | 58 | ~70,600 | ~1,200 | — | — | — | 0 | 0 |
| 2026-10-30 (baseline, after 4.5.0) | 58 | 132,499 | 2,242 | 1,719 | 2,210 | 2,191 | 0 | 0 |

The distribution is flat: the most downloaded package (`Kck.Core.Abstractions`, 3,486) is about twice the median, and
packages nobody would install alone (`*.Abstractions`) lead the list. This is consistent with mostly automated traffic.
External issues and stars are the signals to watch.
