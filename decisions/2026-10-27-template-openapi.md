# Karar: kck-webapi şablonunda API dokümantasyonu

- Tarih: 2026-10-27
- Bağlam: `dotnet new kck-webapi` ile üretilen proje derlenmiyordu: `Program.cs` `AddSwaggerGen` / `UseSwagger` /
  `UseSwaggerUI` çağırıyor ama projede Swashbuckle referansı yok (git geçmişinde hiç eklenmemiş). Uçtan uca deneme
  (Kck.Templates 3.5.0 → yeni proje → `dotnet build`) 3 × CS1061 verdi.
- Ön beyan: aday Microsoft.AspNetCore.OpenApi; ölçütler: ücretli/belirsiz lisans yok, net10.0, EOL yok, açık HIGH/CRITICAL
  paket bildirimi yok, resmi sahip. Alternatifler: Swashbuckle.AspNetCore, dokümantasyonsuz şablon.
- Registry: `OpenApi` / `Swashbuckle` kayıtlı değil.

| Aday | Sürüm | Lisans / sahip | net10.0 | Karar |
|---|---|---|---|---|
| Microsoft.AspNetCore.OpenApi | 10.0.14 (2026-10-25) | MIT / Microsoft, dotnet/aspnetcore (aktif) | evet | **Seçildi** |
| Swashbuckle.AspNetCore | 10.3.1 (2026-10-19) | MIT / domaindrivendev (aktif) | evet | Elendi — Microsoft .NET 9 şablonlarından çıkardı; ek üçüncü taraf |
| Dokümantasyonsuz şablon | – | – | – | Elendi — kullanıcı kararı (WebAPI şablonu için zayıf) |

- Seçim: `Microsoft.AspNetCore.OpenApi` `10.0.*`; `builder.Services.AddOpenApi()` + yalnız Development'ta `app.MapOpenApi()`
  (belge `/openapi/v1.json`). Etkileşimli UI (Swagger UI / Scalar) varsayılan değil; kullanıcı ekler.
- Sürüm gerekçesi: şablon içeriğini Dependabot güncellemez; tam sürüm (10.0.14) bugünkü 3.0.0 hatası gibi eskir.
  `10.0.*` 10.0 hattının en yeni yamasını (güvenlik düzeltmeleri dahil) alır, major sabit kalır. Kck major'ı .NET major'ına
  geçtiğinde (net11) bu satır şablonla birlikte güncellenir.
- EOL: .NET 10 LTS 2028-11-14.
- Kaynaklar (erişim 2026-10-27):
  - https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0 (güncelleme 2026-10-23)
  - https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.openapi/index.json
  - https://api.nuget.org/v3-flatcontainer/swashbuckle.aspnetcore/index.json
  - https://endoflife.date/api/dotnet.json
  - `gh api repos/dotnet/aspnetcore`, `repos/domaindrivendev/Swashbuckle.AspNetCore` (archived=false, MIT)
