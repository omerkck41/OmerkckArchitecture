# Karar: SixLabors.ImageSharp'ın kaldırılması — `IImageProcessor` için yeni kütüphane

- Tarih: 2026-10-31 · Bağlam: bakımcı kısıtı "SixLabors ve ücretli lisans gerektiren hiçbir eklenti kullanılmayacak"
  (2026-10-31); plan: `docs/plans/2026-10-31-sixlabors-kaldirma-plani.md`
- Durum: **Onaylandı 2026-10-31** (kullanıcı, sohbet: D1 (a) çıktı jpeg/png/webp; Linux native paketi tüketicide; takvim: **tek sürüm** —
  ara deprecate sürümü yok, `Kck.Documents.ImageSharp` 5.0.0'da doğrudan kaldırılır, `Kck.Documents.SkiaSharp` aynı sürümde gelir — ayrıntı planda)
- Önceki karar: `2026-10-14-sdk-10.0.402-imagesharp-4.md` ImageSharp 4.1.3 + Community anahtarını seçti; ölçütleri
  "anahtarsız izinli lisans" dışında alternatifleri eledi. Bu karar o ölçütü **zorunlu** yapar; ImageSharp 4.x Release
  derlemesinde lisans anahtarı ister (`SixLabors_ValidateLicense` hedefi) ve Split License 1.0 OSI izinli değildir.

## Ön beyan (Anayasa #3 — araştırmadan önce yazıldı, `tasks/checkpoint.md` 2026-10-31)
- Adaylar: SkiaSharp, Magick.NET, NetVips, PhotoSauce.MagicScaler, StbImageSharp ailesi; kontrol: Imageflow.Net,
  System.Drawing.Common.
- Ölçütler (öncelik sırası): (1) paket + native zincirde OSI izinli lisans, anahtar/ücretli katman/AGPL/RPL/Split → ele,
  LGPL native yalnız belgeli istisnayla ve daha düşük puanla; (2) JPEG/PNG/WebP/GIF/BMP decode+encode kapsamı, eksikler
  sayılabilir; (3) son release ≤ 12 ay, açık HIGH/CRITICAL advisory yok, arşivlenmemiş; (4) RID başına native boyut,
  linux-x64/arm64/musl, tüketicinin ek paket zorunluluğu; (5) API uyumu (aspect-fit resize, kalite, kaynak format
  tespiti, tam decode etmeden boyut); (6) AOT/trim; (7) yaygınlık.
- Kaynaklar: nuget.org registration/flat-container, GitHub API (lisans, push, release, advisories), context7 resmi
  belgeler, resmi README/LICENSE. EOL: kütüphane kaydı yok → son release yaşı.
- Registry (`rules/deprecated-registry.md`, 2026-10-31): adayların hiçbiri kayıtlı değil.

## Adaylar (erişim 2026-10-31)

| Aday | Sürüm (yayın) | Lisans (paket / native) | Encode | Decode | Native yük | Sinyal | Karar |
|---|---|---|---|---|---|---|---|
| **SkiaSharp** | 4.154.0 (2026-10-28) | MIT / Skia BSD-3 (MIT nuspec) | **JPEG, PNG, WebP** — GIF/BMP/ICO encode **yok** (resmi belge: "only three … Jpeg, Png, and Webp are supported for encoding") | JPEG, PNG, WebP, GIF, BMP, ICO, WBMP | Win/macOS otomatik; Linux için `SkiaSharp.NativeAssets.Linux.NoDependencies` (glibc+musl, x64/arm64/arm; nupkg 59,6 MB, publish yalnız hedef RID'i kopyalar; fontconfig gerekmez) | push 2026-10-31, release 2026-10-28, advisory 0, 5.630 yıldız, Mono/Microsoft bakımı | **Seçildi** (D1a) |
| Magick.NET-Q8-x64 | 15.0.0 (2026-10-10) | Apache-2.0 (License.txt doğrulandı) / ImageMagick statik; "incompatible license" bileşenleri dışarıda bırakılmış | hepsi (GIF, BMP, TIFF, AVIF…) | hepsi | 41,7 MB (x64), AnyCPU 84 MB; Linux/macOS fontconfig ister; musl için ayrı özellik | push 2026-10-28, aylık release, advisory 0 (ImageMagick üst akımında sık CVE, hızlı yama) | Elendi — tüketici yükü ve saldırı yüzeyi; **D1b yedek** (tam format eşitliği şartsa) |
| NetVips + NetVips.Native | 3.2.0 (2026-01-01) + 8.18.7 (2026-09-28) | MIT / **LGPL-3.0-or-later** (nuspec `license type="expression"`) | JPEG, PNG, WebP, GIF (cgif), TIFF, HEIF, JXL — **BMP yok** (libvips BMP'yi yalnız ImageMagick yükleyicisiyle yazar, native paketlerde yok) | aynı | 7,6 MB/RID, musl var | push 2026-09-28, 538 yıldız, tek bakımcı | Elendi — native LGPL yeni lisans istisnası ister (ölçüt 1), BMP yok |
| PhotoSauce.MagicScaler | 0.15.0 (2024-12-05) | MIT / codec eklentileri MIT (libjpeg-turbo, libpng, libwebp, giflib) | JPEG, PNG, WebP, GIF (Giflib eklentisi) — **BMP/TIFF Linux'ta yok** ("Notable exceptions are support for BMP and TIFF images") | aynı | eklenti başına paket; Linux glibc x64/arm64 | repo push 2026-10-28 ama **son release 23 ay önce**, 739 yıldız | Elendi — ölçüt 3 (release ≤ 12 ay) ve BMP |
| StbImageSharp + StbImageWriteSharp | 2.30.16 (2026-08-08) + 1.16.7 (2022-06-29) | Unlicense OR MIT / saf .NET | PNG, JPEG, BMP, TGA — **WebP ve GIF yok** | JPEG, PNG, BMP, GIF, TGA, PSD | yok (saf C#) | writer 4 yıl releasesiz, GitHub lisans alanı boş | Elendi — WebP yok, writer bakımsız |
| Imageflow.Net | 0.15.1 (2026-03-31) | **(AGPL-3.0-or-later OR Apache-2.0+)** — Apache-2.0+ ticari lisans | hepsi | hepsi | Rust native | — | Elendi — AGPL/ticari (ölçüt 1) |
| System.Drawing.Common | 10.0.14 | MIT | — | — | — | yalnız Windows (.NET 6+ diğer platformlarda `PlatformNotSupportedException`) | Elendi |
| SixLabors.ImageSharp (mevcut) | 4.1.3 (2026-10-09) | Split License 1.0 (GitHub `NOASSERTION`), Release'te anahtar | hepsi | hepsi | yok | 8 advisory (2026-08/09, 4.1.x'te yamalı) | **Kaldırılıyor** (bakımcı kısıtı) |

## Seçim ve gerekçe
- **SkiaSharp 4.154.0**: tek ölçütü geçmeyen alan GIF/BMP **çıktısı**; girdi olarak GIF/BMP okunmaya devam eder. MIT,
  Microsoft/Mono bakımında, .NET ekosisteminin (MAUI, Avalonia, Uno) ortak native katmanı; lisans kapısı (`.github/
  license-policy.json`) için istisna gerekmez; `SKCodec.Create(stream).Info` ile boyut tam decode etmeden okunur
  (ImageSharp sağlayıcısındaki `Image.LoadAsync` eksikliği de kapanır); P/Invoke tabanlı, AOT dumanı testine eklenebilir.
- Anti-reflex: ilk akla gelen "ImageSharp'ın saf .NET ikizi" yok; saf .NET aday (Stb) WebP/GIF yazamıyor ve bakımsız.
  Tam eşitlik veren tek aday Magick.NET'tir; bedeli tüketiciye 42–84 MB native + ImageMagick saldırı yüzeyi + fontconfig.
- **D1 (kullanıcı kararı):** (a) **SkiaSharp, çıktı formatları jpeg/png/webp** — `gif`/`bmp` istenirse
  `NotSupportedException` (desteklenenleri listeler; mevcut sözleşme "desteklenmeyen ad hata" davranışını zaten tanımlar);
  (b) Magick.NET-Q8 ile tam eşitlik; (c) SkiaSharp + depo içi ~50 satırlık sıkıştırmasız 24-bit BMP yazıcı (bmp korunur, gif düşer).
  Öneri: **(a)**; sunucu tarafı küçük resim/dönüşüm için GIF/BMP çıktısı talebi yok, (c) ihtiyaç çıkarsa minor sürümde eklenebilir.
  **Karar (kullanıcı, 2026-10-31): (a).**

## Planlanan değişiklikler (uygulama planda; bu karar yalnız seçimi bağlar)
- Yeni paket `Kck.Documents.SkiaSharp` (`AddKckDocumentsSkiaSharp()`, `SkiaSharpImageProcessor : IImageProcessor`);
  `Kck.Documents.ImageSharp` **5.0.0'da doğrudan kaldırılır** (tek sürüm kararı; `[Obsolete]` ara sürümü yok — `docs/policies/deprecation.md`'ye
  bakımcı notu); eski 4.x paketleri yayın sonrası nuget.org'da deprecate işaretlenir (bakımcı); geçiş rehberi `docs/migrations/v4-to-v5.md`.
- Test projesi Linux CI için `SkiaSharp.NativeAssets.Linux.NoDependencies` referansı; tüketici belgesi Linux notu.
- CI: 8 workflow'dan `SixLaborsLicenseKey` env'i, `Directory.Build.props` lisans bloğu, `.gitignore sixlabors.lic`,
  `license-policy.json` istisnası, `stryker-documents-imagesharp.json` + mutation matrisi, CONTRIBUTING/licensing/known-issues
  K1 notları kaldırılır; GitHub secret `SIXLABORS_LICENSE_KEY` (Actions + Dependabot) **bakımcı** siler (Kırmızı bölge).

## Kaynaklar (erişim 2026-10-31)
- nuget.org registration: `skiasharp` 4.154.0 (MIT), `skiasharp.nativeassets.linux.nodependencies` 4.154.0,
  `magick.net-q8-x64` / `-anycpu` 15.0.0 (Apache-2.0), `netvips` 3.2.0 (MIT), `netvips.native` 8.18.7 (LGPL-3.0-or-later),
  `photosauce.magicscaler` 0.15.0 (MIT, 2024-12-05), `stbimagesharp` 2.30.16, `stbimagewritesharp` 1.16.7 (2022-06-29),
  `imageflow.net` 0.15.1 ((AGPL-3.0-or-later OR Apache-2.0+)), `sixlabors.imagesharp` 4.1.3, `system.drawing.common` 10.0.14
- GitHub API `repos/<repo>` + `/releases/latest` + `/security-advisories`: mono/SkiaSharp (MIT, push 2026-10-31, v4.154.0,
  0 advisory), dlemstra/Magick.NET (Apache-2.0, 15.0.0 2026-10-10, 0), kleisauke/net-vips (MIT, v3.2.0 2026-01-01, 0),
  saucecontrol/PhotoSauce (MIT, v0.15.0 2024-12-05, 0), StbSharp/StbImageSharp (lisans alanı boş), imazen/imageflow-dotnet
  (AGPL-3.0), SixLabors/ImageSharp (NOASSERTION, 8 advisory 2026-08-20..09-15)
- SkiaSharp encode kapsamı: https://github.com/mono/SkiaSharp/blob/main/documentation/docfx/guides/bitmaps/saving.md
  ("only three of these file formats (Jpeg, Png, and Webp) are supported for encoding"); `binding/SkiaSharp/SKPixmap.cs`
  `Encode` switch (Jpeg/Png/Webp, aksi `false`); paket rehberi https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md
  (Linux.NoDependencies: glibc+musl, "Zero third-party deps", minimal/Alpine/distroless konteynerler)
- Magick.NET: https://raw.githubusercontent.com/dlemstra/Magick.NET/main/License.txt (Apache License 2.0);
  https://github.com/dlemstra/Magick.NET/blob/main/docs/CrossPlatform.md ("statically linked build", "some libraries and
  formats are left unsupported where an incompatible license or other issue prevents it", fontconfig gerekir)
- MagicScaler: https://github.com/saucecontrol/photosauce/blob/master/src/MagicScaler/readme.md ("Notable exceptions are
  support for BMP and TIFF images")
- NetVips: `build/native/NetVips.Native.nuspec` (`LGPL-3.0-or-later`); `_autodocs/api-reference/Image.Generated.md`
  savers listesi (gif, jpeg, png, webp, tiff, heif, jxl, magick…; bmp yok); libvips BMP: https://github.com/libvips/libvips/issues/1528
- EOL: kütüphaneler için endoflife.date kaydı yok → N/A; .NET 10 LTS 2028-11-14
