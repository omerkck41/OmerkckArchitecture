# Karar: kck-webapi şablonunda etkileşimli API arayüzü — Scalar.AspNetCore

- Tarih: 2026-10-30
- Bağlam: DX planı G1.2 (`docs/plans/2026-10-30-dx-hero-module-plan.md`), ADR-0029 D3. Şablon .NET 10'un
  `Microsoft.AspNetCore.OpenApi` belgesini üretiyor; Development'ta tarayıcıdan denenebilir bir arayüz "ilk 60 saniye"
  deneyiminin parçası. Negatif gereksinim (plan §13): şablon varsayılan olarak dışarıya istek/telemetri göndermez.
- Pre-commitment (araştırmadan önce): ölçütler — izin verici lisans, son 2 sürümde lisans değişmemiş · son 12 ayda
  aktif · paketi etkileyen açık HIGH/CRITICAL yok · .NET 10 + Microsoft.AspNetCore.OpenApi ile resmi kullanım yolu ·
  kayıtta tam ad ve resmi depo eşleşmesi · `rules/deprecated-registry.md`'de yok · varsayılanda dış istek yok ya da
  kapatılabilir. Kaynaklar: context7 (resmi belge), nuget.org kayıt API'si, GitHub REST API.
- Registry taraması: `scalar|swashbuckle|swagger|nswag` → eşleşme yok.

## Adaylar
| Aday | Sürüm (erişim 2026-10-30) | Lisans | Son push / sürüm | Paketi etkileyen açık HIGH/CRITICAL | Karar |
|---|---|---|---|---|---|
| A — Scalar.AspNetCore | 2.18.6 (2026-10-30), 2.18.5 (2026-10-28) | MIT (son 3 sürüm) | 2026-10-30 | Yok (aşağıda) | **Seç — 2.18.5** |
| B — Swashbuckle.AspNetCore.SwaggerUI | 10.3.1 (2026-10-19) | MIT | 2026-10-28 | Yok | Ele |
| C — Arayüz yok: OpenAPI JSON + `.http` dosyası | — | — | — | — | Plan B; `.http` her durumda şablonda |

## Gerekçe
- A'nın arayüzü her işlem için kimlik doğrulama ön doldurmalı bir API istemcisi ve C# `HttpClient` örnekleri
  sunuyor; B yalnız klasik Swagger UI. İkisi de varlıkları paket içinden sunuyor; DX hedefi için A daha güçlü.
- **Dış istek / telemetri (A):** varsayılan `ProxyUrl` null (Scalar'ın kendi testi bunu doğruluyor); arayüz varlıkları
  paketin içinde, CDN'den yüklenmiyor; telemetri yalnız Scalar'ın kendi platformunda yüklenen eklentiyle — kütüphane
  olarak kullanımda izleme yok. **Tek dış istek varsayılan font CDN'i → şablonda `DisableDefaultFonts()`.**
- **Güvenlik bildirimleri:** scalar/scalar monorepo'sunda 2026-09-25 ve 2026-10-21'de 8 HIGH/CRITICAL bildirim var;
  hepsi npm paketleri (`@scalar/mock-server`, `@scalar/workspace-store`, `@scalar/openapi-to-markdown`) veya Scalar'ın
  barındırdığı CORS proxy (proxy.scalar.com) için. NuGet `Scalar.AspNetCore` etkilenmiyor; şablon proxy kullanmıyor.
  Bildirim sıklığı yüksek olduğu için arayüz **yalnız Development'ta** eşlenir (`app.Environment.IsDevelopment()`).
- **Sürüm:** 2.18.6 aynı gün yayımlandı; haftada birkaç sürüm çıkıyor. Aynı gün sürümü yerine 2.18.5 sabitlenir;
  güncellemeleri Dependabot önerir (Directory.Packages.props üzerinden şablona damgalanır — plan G1.1).
- **Paket kimliği:** nuget.org kaydı `Scalar.AspNetCore`, `projectUrl` https://scalar.com/, kaynak
  github.com/scalar/scalar `integrations/dotnet/aspnetcore`; bağımlılık yok (yalnız ASP.NET Core framework başvurusu).
- EOL: N/A (sürekli yayın; .NET 10 LTS ile destekleniyor).

## Kaynaklar (erişim 2026-10-30)
- https://github.com/scalar/scalar/blob/main/documentation/integrations/aspnetcore/integration.md — "Scalar uses local
  assets by default", fontlar CDN'den, `DisableDefaultFonts()`
- https://github.com/scalar/scalar/blob/main/integrations/dotnet/aspnetcore/tests/Scalar.AspNetCore.Tests/ScalarOptionsMapperTests.cs
  — varsayılan `ProxyUrl` null
- https://github.com/scalar/scalar/blob/main/documentation/configuration.md — telemetri yalnız analitik eklentisiyle
- https://api.nuget.org/v3/registration5-gz-semver2/scalar.aspnetcore/index.json — sürümler, lisans, tarih
- `gh api repos/scalar/scalar` (MIT, arşivlenmemiş, push 2026-10-30) ve `/security-advisories` (etkilenen paketler)
- `gh api repos/domaindrivendev/Swashbuckle.AspNetCore` (MIT, v10.3.1 2026-10-19, bildirim yok)

## Uygulama
- `Directory.Packages.props`: `Scalar.AspNetCore` 2.18.5 (yalnız şablon damgası için; kütüphane paketleri başvurmaz).
- Şablon `Program.cs`: Development'ta `app.MapOpenApi(); app.MapScalarApiReference(o => o.DisableDefaultFonts());`
- Lisans kapısı: şablon çıktısı repo lock dosyalarında değil; Scalar repo'da yalnız damgalama için listelenirse
  lisans kapısı MIT'i doğrular.
