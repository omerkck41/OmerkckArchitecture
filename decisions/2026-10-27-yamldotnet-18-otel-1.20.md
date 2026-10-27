# Karar: YamlDotNet 16 → 18 ve OpenTelemetry 1.15 → 1.20 (Kck 4.0)

- Tarih: 2026-10-27 · Bağlam: ADR-0028 K7, v4 planı PR 2 (H11-a, küçük majorlar). Dependabot #167 (OTel) yerine geçer.
- Ön beyan: ölçütler — izin verici lisans, net10.0, arşivlenmemiş repo, hedef sürümü etkileyen açık güvenlik bildirimi yok,
  Kck public API'si kırılmaz (paket doğrulama), mevcut testler yeşil. Alternatif: mevcut sürümlerde kalmak.
- Registry: `YamlDotNet`, `OpenTelemetry` deprecated-registry'de yok.

| Paket | Önce → Sonra | Lisans | Sinyal | Karar |
|---|---|---|---|---|
| YamlDotNet | 16.3.0 → 18.1.0 (2026-06-26) | MIT | repo aktif (2026-10-01), güvenlik bildirimi 0 | **Seçildi** |
| OpenTelemetry.Api / Extensions.Hosting / Exporter.OpenTelemetryProtocol | 1.15.3 → 1.20.1 (2026-10-20) | Apache-2.0 | repo aktif; 8 bildirimin hepsi ≤ 1.15.2 sürümleri için (1.20.1 etkilenmiyor) | **Seçildi** |
| OpenTelemetry.Instrumentation.AspNetCore / Http | 1.15.2 / 1.15.1 → 1.20.1 | Apache-2.0 | aynı | **Seçildi** |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.15.1-beta.1 → 1.20.1-beta.1 | Apache-2.0 | kararlı sürümü hâlâ yok (NU5104 bastırması sürüyor) | **Seçildi** (beta, mevcut durumla aynı) |
| Mevcut sürümlerde kalmak | — | — | — | Elendi: v4 sonrası 4.x yamalarına bağımlılık yükü bırakır; Kck API'si zaten değişmiyor |

## Kck'ya etkisi
- **Public API:** değişmiyor — paket doğrulama (taban 3.6.0) CP hatası vermedi; suppression gerekmez.
- **YamlDotNet:** Kck yalnız dahili `DeserializerBuilder` → `Dictionary<string, object>` kullanıyor. 18.0'ın tek kırıcı
  değişikliği (`ITypeInspector`'a 2 metot) Kck'yı etkilemez. 17–18 varsayılan özyineleme sınırı (130) ve `MergingParser`
  olay sınırı (100k) derin iç içe YAML'a karşı DoS korumasıdır; 130'dan derin yerelleştirme dosyası hata verir.
- **OpenTelemetry (tüketici davranışı, geçiş rehberine):**
  - OTLP exporter'ın kendi oluşturduğu `HttpClient` artık yönlendirme (3xx) izlemez; 3xx başarısız gönderim sayılır (1.20.1).
  - Tek gönderim üst sınırı 128 MiB → 64 MiB (`OtlpExporterOptions.MaxRequestSizeBytes`, 1.18).
  - Öznitelik sınırları: `byte[]` öznitelikler değer uzunluğu sınırına kırpılır; olay/bağlantı öznitelik sayısı
    `OTEL_ATTRIBUTE_COUNT_LIMIT`'e düşer (1.20.0).

## Doğrulama (2026-10-27)
- Kod değişikliği gerekmedi. `Kck.Localization.Yaml.Tests` 13/13, `Kck.Observability.OpenTelemetry.Tests` 10/10.
- Solution: `restore --force-evaluate` + `--locked-mode`, Release build 0 hata, birim testleri 0 başarısız, pack + paket
  doğrulama hatasız, lisans kapısı PASS (227 paket).
- Atlandı: gerçek bir OTLP toplayıcısına uçtan uca gönderim (yerelde collector yok).

## Kaynaklar (erişim 2026-10-27)
- https://github.com/aaubry/YamlDotNet/releases (v17.0.0, v17.1.0, v18.0.0, v18.1.0)
- https://github.com/open-telemetry/opentelemetry-dotnet/releases (core-1.16.0 … core-1.20.1)
- `gh api repos/<repo>/security-advisories` (YamlDotNet 0; opentelemetry-dotnet 8, hepsi ≤ 1.15.2)
- https://api.nuget.org/v3/registration5-semver1/yamldotnet/18.1.0.json, `.../opentelemetry.api/1.20.1.json`
- EOL: endoflife.date kaydı yok → N/A; .NET 10 LTS 2028-11-14
