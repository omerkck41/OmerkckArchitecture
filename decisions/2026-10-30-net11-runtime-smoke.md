# Karar: .NET 11 runtime smoke (H19) RC3 ile şimdi

- Tarih: 2026-10-30
- Bağlam: Yol haritası H19 "net10 testleri net11 runtime'da roll-forward ile; `global.json` değişmez" diyor ve .NET 11
  GA'yı bekliyordu. Kullanıcı açık işlerin hepsinin bitirilmesini istedi (2026-10-30).
- Pre-commitment: ölçütler — resmi Microsoft sürüm metadatası, üretimde desteklenen ("go-live") sürüm, güvenlik
  güncellemesi içeren en son yayın; `global.json` ve hedef framework değişmez; iş zorunlu kontrol olmaz.
- Adaylar:
  - A — Şimdi .NET 11 RC3 (go-live) runtime'ı ile; GA çıkınca yalnız sürüm satırı değişir. Seç: uyumsuzluk GA'dan
    önce görülür.
  - B — GA'yı beklemek (Kasım 2026): H19 açık kalır, uyumsuzluk yayından sonra görülür. Ele.
  - C — Hedef framework'e `net11.0` eklemek (multi-target): yol haritası K7 kararıyla reddedilmişti. Ele.
- Sürüm (https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/11.0/releases.json, erişim 2026-10-30):
  `11.0.0-rc.3` — yayın 2026-10-25, support-phase `go-live`, security: true; SDK `11.0.100-rc.3.26522.148`, runtime
  `11.0.0-rc.3.26522.148`.
- EOL: .NET 11 STS; GA sonrası destek takvimi endoflife.date'te (henüz kayıt yok). .NET 10 LTS 2028-11-14.
- `rules/deprecated-registry.md`: kayıt yok.
- Çalışma: `build-test.yml` `net11-runtime` işi — .NET 10 SDK ile derler; yalnız kontrol ve test adımlarında
  `DOTNET_ROLL_FORWARD=LatestMajor` + `DOTNET_ROLL_FORWARD_TO_PRERELEASE=1`; bir adım net10.0 uygulamasının runtime
  11'de çalıştığını doğrular. GA'da: SDK satırını GA sürümüyle değiştir, `TO_PRERELEASE` satırlarını kaldır.
