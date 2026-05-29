# Karar: OpenTelemetry.Instrumentation.EntityFrameworkCore 1.12.0-beta.2 → 1.15.1-beta.1

- **Tarih:** 2026-05-30
- **Bağlam:** `dotnet list package --outdated` bu paketi "Kaynaklarda bulunamadı" gösterdi;
  araştırmada paketin **stable sürümü olmadığı** (tasarım gereği beta — deneysel semantic
  conventions) doğrulandı. Bizim sürüm (1.12.0-beta.2) OTel stack'in geri kalanından (1.15.x)
  geride kalıyordu.
- **Karar:** 1.15.1-beta.1'e yükselt → OTel stack version-alignment.
- **Sürüm:** 1.15.1-beta.1 (yayın: 2026-04-21; paketin en güncel pre-release'i)
- **Kaynaklar:**
  - https://www.nuget.org/packages/OpenTelemetry.Instrumentation.EntityFrameworkCore (erişim: 2026-05-30) — stable yok, latest 1.15.1-beta.1
  - https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/3026 (erişim: 2026-05-30) — SetDbStatementForText kaldırıldı
- **Breaking change + çözüm:** 1.13+ `EntityFrameworkInstrumentationOptions.SetDbStatementForText`
  özelliğini kaldırdı; davranış artık **her zaman açık ve sorgu sanitize ediliyor** (literal'ler `?`).
  `KckOpenTelemetryBuilder.cs`: `.AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true)`
  → `.AddEntityFrameworkCoreInstrumentation()`. Yeni davranış `rules/observability.md` (db.statement
  hassas veri sanitize) ile daha uyumlu.
- **Doğrulama:** build 0 hata; Observability testleri 10/10 geçti.
- **EOL:** N/A (paket kalıcı pre-release hattında; stable çıkana dek beta'lar arası breaking change olası).
- **Not:** Beta-only paket; gelecekte stable çıkarsa tekrar değerlendir.
