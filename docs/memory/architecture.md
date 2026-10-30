# Mimari Harita — OmerkckArchitecture

> docs/memory/architecture.md · Plan etki haritalarinin kaynagi (docfx'te yayinlanmaz). Sozlesme degisirse burada
> "KIRICI" isaretlenir. Olusturuldu: 2026-10-30 (DX planı G0.1).

## Katmanlar
`Abstractions` (sozlesmeler, bagimliliksiz ya da yalniz `Kck.Core.Abstractions`) → `Providers` (somut teknoloji) →
`Bundles` (barindirma modeline gore varsayilanlar) → `Kck.Templates` (`dotnet new`).

| Katman | Paketler | Kural |
|---|---|---|
| Abstractions | 17 (`Kck.*.Abstractions`) | Providers'a basvurmaz |
| Providers | 37 | Kendi Abstractions'ina + `Kck.Core.Abstractions`'a basvurur; birbirine yalniz Abstractions uzerinden |
| Bundles | `WebApi`, `MinimalApi`, `WorkerService` | Provider secer, yapilandirmayi `Kck:*` anahtarlarindan okur |
| Templates | `Kck.Templates` (`kck-webapi`) | Paketleme aninda Kck surumu damgalanir (`KCK_PACKAGE_VERSION`) |

## Kahraman modul — `Kck.Persistence.EntityFramework`
- Basvurur: `Kck.Core.Abstractions` (`IDomainEvent`, `FullEntity`, `ISoftDeletable`), `Kck.Persistence.Abstractions`
  (`IRepository`, `IUnitOfWork`, `QueryOptions`, `IPaginate`, `IFilterPropertyWhitelist`).
- Basvuran: `Kck.Bundle.WebApi`; `Kck.Pipeline.Mediator` yalniz `Kck.Persistence.Abstractions`'a (TransactionBehavior →
  `IUnitOfWork`), EF saglayicisina degil.
- Giris noktasi: `AddKckPersistence(p => p.UseEntityFramework<T>(…) | UseEntityFrameworkPool<T>(…)
  .AddAuditInterceptor().AddDomainEventDispatch<TDispatcher>())`.
- Sozlesmeler (davranis): soft-delete silme kurallarina gore yayilir (docs/providers/persistence.md "Soft-delete kapsami");
  domain event'ler transaction commit'ine baglidir, rollback'te atilir, hata olursa entity'ye geri eklenir (en-az-bir-kez);
  havuzlu context'te kiralama degisince bekleyen event'ler atilir.
- Bilinen bosluk: hazir `IDomainEventDispatcher` uygulamasi yok → DX plani G1.0 (ADR-0030).

## Capraz kurallar
- Public API: `PublicAPI.Shipped/Unshipped.txt` (RS0016/RS0017); paket dogrulamasi son yayina karsi (`Directory.Build.props`).
- Merkezi paket yonetimi + lock dosyalari; uyarilar hata.
- Native AOT: `IsAotCompatible` yalniz CI'daki AOT smoke'un dogruladigi saglayicilarda (samples/Kck.AotSmoke).
