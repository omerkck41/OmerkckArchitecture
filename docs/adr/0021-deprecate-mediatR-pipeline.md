# ADR-0021: Kck.Pipeline.MediatR Deprecasyonu

Tarih: 2026-05-07
Durum: Onaylandi
İlgili: Library Strategy §1.3

## Bağlam

FAZ-8 kapsamında `Kck.Pipeline.MediatR` eklendi (MediatR 14.x wrapper).
FAZ-9 kapsamında `Kck.Pipeline.Mediator` eklendi (Mediator.Abstractions wrapper,
source-generator tabanlı, AOT uyumlu, sıfır-bağımlılık).

İki paket aynı işlevi üstlendiğinden ikisini birden sürdürmek bakım yükü yaratır.
`Mediator.Abstractions`, `.NET 10+ first` konumlandırmasıyla daha uyumludur.

## Karar

`Kck.Pipeline.MediatR` deprecated olarak işaretlendi (`KCK0200`).
Paket kaldırılmıyor — backward compatibility korunur. Tüketiciler uyarı alır
ve `docs/migrations/mediatR-to-mediator.md` kılavuzuna yönlendirilir.

## Gerekçe

1. **Tek sorumluluk:** İki paralel pipeline wrapper bakım yükü oluşturur.
2. **AOT uyumu:** `Mediator` source-generator ile AOT-safe; `MediatR` runtime reflection kullanır.
3. **Sıfır-bağımlılık:** `Mediator.Abstractions` herhangi bir framework bağımlılığı gerektirmez.
4. **Yol haritası tutarlılığı:** Library Strategy §1.3 bu geçişi açıkça tanımlar.

## Alternatifler

1. **Her ikisini birlikte sürdür:** Reddedildi — ikileşen bakım yükü.
2. **MediatR'ı hemen kaldır:** Reddedildi — breaking change; tüketicilere geçiş süresi tanınmalı.

## Sonuçlar

- `KckPipelineBuilder` ve `AddKckPipeline()` `[Obsolete(DiagnosticId="KCK0200")]` ile işaretlendi.
- `KCK0200` `WarningsNotAsErrors`'e alındı — uyarı, hata değil.
- Migration kılavuzu: `docs/migrations/mediatR-to-mediator.md`
- `Kck.Pipeline.MediatR` bir sonraki major sürümde (v3.0+) kaldırılacak.
