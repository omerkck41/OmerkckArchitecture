# Karar: Kck.Persistence.EntityFramework → Microsoft.EntityFrameworkCore.Relational

- Tarih: 2026-10-28
- Bağlam: `DomainEventDispatchInterceptor`, açık bir transaction içinde kaydedilen domain event'leri commit'e kadar
  bekletmeli ve rollback'te atmalı (adversarial review F4, PR #195 sonrası). Commit/rollback bildirimini veren
  `IDbTransactionInterceptor`, `Microsoft.EntityFrameworkCore.Relational` paketindedir; paket bugün yalnız
  `Microsoft.EntityFrameworkCore`'a bağlı.
- Pre-commitment: aday Relational 10.0.14; kaynak nuget.org kaydı + context7 (EF docs) + endoflife.date; eleme:
  registry kaydı, 10.0.x'in en yenisi olmaması, EOL, kimlik/depo uyuşmazlığı.
- Adaylar:
  - A — `Microsoft.EntityFrameworkCore.Relational` 10.0.14 doğrudan bağımlılık: her transaction yolu (IUnitOfWork ve
    doğrudan `Database.BeginTransaction`) kapsanır. Seç.
  - B — bağımlılık yok, commit/rollback'i yalnız `EfUnitOfWork` üzerinden yakala: doğrudan
    `Database.BeginTransaction` kullananlarda event'ler yanlış zamanda dağıtılmaya devam eder. Ele.
- Seçim: A (kullanıcı onayı 2026-10-28).
- Sürüm: 10.0.14 (`Directory.Packages.props`, EF Core ile aynı satır; Dependabot EF grubuyla birlikte günceller).
- Doğrulama:
  - nuget.org: 10.0.x en yenisi 10.0.14, `Microsoft.EntityFrameworkCore` de 10.0.14; authors Microsoft, lisans MIT,
    repo https://github.com/dotnet/dotnet (https://api.nuget.org/v3-flatcontainer/microsoft.entityframeworkcore.relational/index.json,
    erişim 2026-10-28)
  - EF Core belgesi: `IDbTransactionInterceptor` commit/rollback/failure olaylarını yakalar
    (https://github.com/dotnet/entityframework.docs/blob/main/entity-framework/core/logging-events-diagnostics/interceptors.md,
    context7, erişim 2026-10-28)
  - `rules/deprecated-registry.md`: EF kaydı yok.
- EOL: .NET 10 / EF Core 10 — 2028-11-14 (https://endoflife.date/dotnet, erişim 2026-10-28).
- Etki: SqlServer/Npgsql/Sqlite sağlayıcıları Relational'a zaten bağlı; yalnız InMemory/Cosmos kullanan tüketicilere
  yeni bir assembly gelir. Lisans kapısı MIT'yi kabul eder.
