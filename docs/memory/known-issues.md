# Bilinen Sorunlar ve Teknik Borc — OmerkckArchitecture

> docs/memory/known-issues.md · Kapanan madde silinmez: Durum "kapandi: <commit>" olur.
> Standart: ~/.claude/skills/lifecycle/references/memory/README.md (diger hafiza dosyalari icin `/context-prime`).

| # | Tur | Aciklama | Etki | Gecici cozum | Durum |
|---|---|---|---|---|---|
| K1 | risk-kabul / takvim | Six Labors lisans yenileme penceresi **2027-10-14**; lisans 2028-01-12'de biter. Kaynak: decisions/2026-10-14-sdk-10.0.402-imagesharp-4.md (ImageSharp 4.x Release derlemesi lisans anahtari ister) | Kck.Documents.ImageSharp derlemesi | Yok — 4.1.3 + Community anahtari (`SIXLABORS_LICENSE_KEY`); lisans kapisinda belgeli istisna (`.github/license-policy.json`) | acik (2027-10-14'te yenile) |
| K3 | ortam | Testcontainers kullanan testler (`[Trait("Category", "Integration")]`, ornek `EfRepositoryIntegrationTests`) Docker ister; Docker kapaliyken `dotnet test` exit 1 verir | Yerel test kosusu | `dotnet test -c Release --filter "Category!=Integration"`; Integration CI'da kosar | acik (2026-10-28) |
| K2 | risk-kabul / takvim | MediatR 14.1.0 (RPL-1.5/ticari) yalniz deprecated `Kck.Pipeline.MediatR` icin; lisans kapisinda vadeli istisna **2027-01-31** — v4.0.0'da paket kaldirilmazsa kapi duser. Kaynak: decisions/2026-10-26-license-gate.md | Kck.Pipeline.MediatR tuketicileri | `Kck.Pipeline.Mediator` (MIT) | kapandi: v4 PR 4 (paket ve lisans istisnasi kaldirildi, 2026-10-27) |
