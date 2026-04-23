# ADR-0008: Split `Kck.Exceptions` into `Kck.Exceptions.Abstractions`

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

`Kck.Exceptions` paketi istisna tiplerini (`BusinessException`, `ValidationException`, `NotFoundException`, vb.) iceriyor. Bu tipler hem ASP.NET Core bundle'indaki middleware'lerde hem de saf `netstandard` kutuphanelerde (pipeline, domain code) kullaniliyor. Paket ise `Providers/` altinda yer aliyordu — Abstractions → Providers → Bundles akisina aykiri.

Sorun:
- `Kck.Pipeline.MediatR` abstraction'a bagimli olmak istiyor ama bir "provider"e referans vermek zorunda kaliyordu.
- Kullanici code'unda istisna tiplerinden emin olmak icin "Providers/Kck.Exceptions" reference'i gerekiyor — mimari katmanlamayi bozuyor.

## Karar

`Kck.Exceptions` fiziksel olarak `src/Providers/Kck.Exceptions/` dizininden `src/Abstractions/Kck.Exceptions.Abstractions/` dizinine tasindi ve yeniden adlandirildi. Source-level namespace `Kck.Exceptions` korundu (zero source-level break).

Degisiklikler:
- `src/Abstractions/Kck.Exceptions.Abstractions/Kck.Exceptions.Abstractions.csproj` (yeni konum).
- `Kck.Exceptions.AspNetCore.csproj` -> `Kck.Exceptions.Abstractions` reference.
- `Kck.Pipeline.MediatR.csproj` -> `Kck.Exceptions.Abstractions` reference.
- `Kck.Bundle.WebApi.csproj` -> direkt reference kaldirildi (transitive: `Kck.Exceptions.AspNetCore` -> `Kck.Exceptions.Abstractions`).
- `OmerkckArchitecture.sln` path guncellemesi.

## Alternatiflier Degerlendirildi

1. **Namespace degisikligi (`Kck.Exceptions` -> `Kck.Exceptions.Abstractions`):** Reddedildi — tum consumer'larin `using` satirlarini degistirmek gerekir, yuksek kirilma riski.
2. **Mevcut lokasyonda birakma:** Reddedildi — mimari katmanlama ihlali kalici olarak projenin temellerine sirayet ederdi.
3. **Iki ayri paket (Abstractions + Providers):** Reddedildi — istisna tipleri provider'a bagli olmayan puro DTO'lar, split gereksiz.

## Sonuclar

Olumlu:
- Abstractions → Providers → Bundles mimari akisi korundu.
- Pipeline code'u artik "provider"e referans vermek zorunda degil.
- Source-level consumer'lar etkilenmedi (`using Kck.Exceptions;` calismaya devam ediyor).

Olumsuz:
- csproj dosyalarinda reference yolu degisti — downstream kullanicilarda rebuild gerekli.
- Git gecmisi `src/Providers/Kck.Exceptions/` altinda kaliyor; `git log --follow` ile takip edilebilir.
