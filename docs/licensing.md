# Third-party licensing

Kck is MIT-licensed. Its rule for dependencies: **no dependency may require a paid license**, and the default path a
consumer takes should be free and permissive. The CI `license-audit` workflow enforces this on every push, pull
request and weekly schedule. Decision record: [decisions/2026-10-26-license-gate.md](../decisions/2026-10-26-license-gate.md).

## How the gate works

`.github/scripts/license_gate.py` reads every `packages.lock.json` (the exact dependency set CI restores with
`--locked-mode`), opens each package's nuspec in the NuGet global packages folder and compares its declared license with
[`.github/license-policy.json`](../.github/license-policy.json). The gate is **fail-closed**: an unknown license, an
unreadable nuspec, a missing lock file or an expired exception fails the job.

| Declared license | Result |
|---|---|
| SPDX expression on the allowed list (MIT, Apache-2.0, BSD-2/3-Clause, MS-PL, ISC, PostgreSQL, Zlib, Unlicense, 0BSD) | allowed |
| `OR` expression with at least one allowed option (e.g. `Apache-2.0 OR MPL-2.0`) | allowed |
| License **file** or **URL** | allowed only if reviewed in the policy; files are pinned by the SHA-256 of their text, so a changed license text fails the gate |
| Anything else (GPL, RPL, commercial EULA, `AND` / `WITH` expressions) | violation unless listed as a documented exception |

## Documented exceptions

| Package | License | Why it is acceptable | Expires |
|---|---|---|---|
| `Hangfire*` (used by `Kck.BackgroundJobs.Hangfire`) | LGPL-3.0-or-later or commercial | Free under LGPL-3.0; consumed unmodified as a NuGet dependency. `Kck.BackgroundJobs.Quartz` (Apache-2.0) is the permissive alternative. | - |
| `SixLabors.ImageSharp` (used by `Kck.Documents.ImageSharp`) | Six Labors Split License 1.0 | Apache-2.0 applies to this open-source project and to anyone consuming ImageSharp **through** Kck ("Transitive Package Dependency" clause). The maintainer's Release build key is a free Community license (until 2028-01-12). | - |
| `MediatR` (used only by the deprecated `Kck.Pipeline.MediatR`) | RPL-1.5 or Lucky Penny commercial | Temporary. MediatR 13+ is commercially licensed; `Kck.Pipeline.MediatR` is removed in v4.0.0. Use `Kck.Pipeline.Mediator` (MIT). | 2027-01-31 |

### Notes for consumers

- If your application references **SixLabors.ImageSharp directly** (not only through `Kck.Documents.ImageSharp`), the
  Split License applies to your direct use; evaluate it for your organisation.
- If you reference `Kck.Pipeline.MediatR`, MediatR's own license terms apply to your application. Migrate to
  `Kck.Pipeline.Mediator` ([guide](migrations/mediatR-to-mediator.md)).
- Cloud providers (`AwsS3`, `AzureBlob`, `SendGrid`, `AmazonSes`, Azure Key Vault / App Configuration, Elasticsearch
  server) have permissively licensed client SDKs; any cost comes from the service you choose, not from a license.

This page describes the project's policy; it is not legal advice.

## When the gate fails on your pull request

1. Read the job summary: it names the package, version, and the license kind (expression, file + SHA-256, or URL).
2. Prefer a permissively licensed alternative or keep the previous version.
3. If the license is acceptable (for example a license file whose text is plain MIT), add or update the entry in
   `.github/license-policy.json` with the SHA-256 printed by the gate, and explain it in the pull request.
4. New exceptions (copyleft or commercial terms) need the maintainer's approval and a row in the table above.
