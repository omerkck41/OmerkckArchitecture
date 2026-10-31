# SixLabors.ImageSharp'ın Kaldırılması — Çalışma Planı

- Tarih: 2026-10-31 · Kademe: **K3** (yeni bağımlılık: SkiaSharp; paket yüzeyi: yeni paket + deprecate; CI secret kaldırma) ·
  Profil: Kurumsal (TIER 3)
- Üst karar: bakımcı kısıtı "SixLabors ve ücretli lisans gerektiren hiçbir eklenti kullanılmayacak" (2026-10-31).
  Karar artefaktı (Anayasa #4): [decisions/2026-10-31-image-library-replacement.md](../../decisions/2026-10-31-image-library-replacement.md)
  — seçim **SkiaSharp 4.154.0 (MIT)**; yedek Magick.NET-Q8 (Apache-2.0).
- Durum: **kararlar alındı 2026-10-31** (kullanıcı, sohbet: D1 (a) jpeg/png/webp · D2 (b) 4.9 deprecate → 5.0 kaldır · D3 (a) Linux
  native tüketicide). **Üst karar (kullanıcı, 2026-10-31, sonra): tüm geliştirmeler tek sürümde (5.0.0)** → ara deprecate sürümü yok,
  `Kck.Documents.ImageSharp` 5.0.0'da **doğrudan kaldırılır** (S2/S3 buna göre değişti). Dosya kabulü ve commit bekliyor.
  Kardeş plan: [Provider sağlık + atıl kod](2026-10-31-provider-saglik-ve-atil-kod-plani.md).
- Lisans taraması (2026-10-31, lisans kapısı 257 paket): ücretli/anahtarlı lisans gerektiren **tek** bağımlılık SixLabors.ImageSharp 4.1.3.
  Diğer istisnalar LGPL-3.0 (Hangfire ailesi — ücretsiz, dinamik bağlama; `dotnet-sonarscanner` — yalnız CI aracı); ücretli değil,
  bilgi notu olarak kalır.

## 1. Hedef
`IImageProcessor` sözleşmesi aynı kalsın; `Kck.Documents.ImageSharp` yerine lisans anahtarı ve ücretli katmanı olmayan, MIT lisanslı
`Kck.Documents.SkiaSharp` sağlayıcısı gelsin; depo, CI ve belgelerde Six Labors'a dair hiçbir kablolama (lisans dosyası, secret, env,
politika istisnası) kalmasın; tüketiciler deprecation politikasına uygun geçiş yolu bulsun.

## 2. Mevcut kullanım haritası (2026-10-31 taraması)
| Alan | Yer | Kaldırma adımı |
|---|---|---|
| Sağlayıcı | `src/Providers/Kck.Documents.ImageSharp` (`ImageSharpProcessor.cs` 105 satır, DI uzantısı, README.nuget.md, lock) | S2 deprecate → S4 sil |
| Test | `tests/Kck.Documents.ImageSharp.Tests` (12 test: resize, convert, alias, bilinmeyen format, boş format, TIFF kaynak) | S1'de SkiaSharp'a taşınır, S4 sil |
| README derlemesi | `samples/Kck.ReadmeSnippets` (ProjectReference + 3 üretilmiş snippet) | S1 yeni snippet, S4 eski kaldır |
| Paket sürümü | `Directory.Packages.props` `SixLabors.ImageSharp 4.1.3` | S4 sil, solution-wide lock yenile |
| Lisans kablolaması | `Directory.Build.props` `SixLaborsLicenseFile` bloğu; `.gitignore` `sixlabors.lic`; yerel `sixlabors.lic` (git dışı) | S4 sil; yerel dosyayı **bakımcı** siler |
| CI | 8 workflow `SixLaborsLicenseKey: ${{ secrets.SIXLABORS_LICENSE_KEY }}` (benchmark, build-test ×3 adım, codeql, docs ×2, mutation ×2, nuget-publish, sonar, template-smoke) + açıklama satırları; `dependabot-lockfile.yml` yorumu | S4 sil |
| GitHub secret | `SIXLABORS_LICENSE_KEY` (Actions + Dependabot) | **Kırmızı bölge — bakımcı siler** (3. parti hesap durumu) |
| Lisans politikası | `.github/license-policy.json` `SixLabors.ImageSharp` istisnası (SHA sabit) | S4 sil → kapı istisnasız geçer |
| Mutation | `stryker-documents-imagesharp.json` + `mutation.yml` matris satırı | S1 `stryker-documents-skiasharp.json`, S4 eski sil |
| Belgeler | `docs/providers/documents.md` (ImageSharp bölümü, "native bağımlılık yok" iddiası), `docs/providers/README.md`, kök `README.md` tablo, `docs/licensing.md` (satır + "doğrudan referans" notu), `docs/memory/known-issues.md` K1 (lisans yenileme 2027-10-14), `CONTRIBUTING.md` (Release derleme anahtarı notu), `docs/migrations/v3-to-v4.md` §6, `.github/docs-english.txt` satırı, `docs/adr/0028-v4-scope.md` K6 (tarihsel, kalır) | S1 ekle, S4 düzelt |
| Karar kayıtları | `decisions/2026-10-14-sdk-10.0.402-imagesharp-4.md` (tarihsel, kalır; "yenileme 2028-01-12" notu artık geçersiz → kapanış notu eklenir) | S4 not |

## 3. Kullanıcı kararları (onay paketi)
| # | Karar | Seçenekler | Öneri · **Karar (kullanıcı, 2026-10-31)** |
|---|---|---|---|
| D1 | Çıktı formatı kapsamı (SkiaSharp yalnız JPEG/PNG/WebP yazar; GIF/BMP **okur**, yazamaz) | (a) jpeg/png/webp; `gif`/`bmp` çıktısı `NotSupportedException` (desteklenenleri listeler) · (b) Magick.NET-Q8 ile tam eşitlik (+42–84 MB native, ImageMagick saldırı yüzeyi, fontconfig) · (c) SkiaSharp + depo içi ~50 satır sıkıştırmasız 24-bit BMP yazıcı (bmp korunur, gif düşer) | (a) — sunucu tarafı küçük resim/dönüşüm için GIF/BMP çıktı talebi yok; (c) gerekirse minor'da eklenebilir · **Karar: (a)** |
| D2 | Kaldırma takvimi (`docs/policies/deprecation.md`: `[Obsolete]` en az iki minor sürüm kaynakta kalır, major'da silinir) | (a) politika: 4.9.0 SkiaSharp + ImageSharp `[Obsolete]`; 4.10.0 korunur; **5.0.0** silinir (anahtar ve secret 5.0'a kadar CI'da kalır — anahtar ücretsiz Community) · (b) hızlandırılmış: 4.9.0 deprecate → bir sonraki yayın doğrudan 5.0.0 (16 `[Obsolete]` üyeyle birlikte); politikadaki "iki minor" bakımcı kararıyla tek minor'a iner | (b) — kısıt "hiç kullanılmasın"; v5 kaldırma listesi zaten dolu, 4.10 ara sürümü yapay · **Karar: (b) → üst karar: tek sürüm**, deprecate sürümü de yok: ImageSharp paketi 5.0.0'da doğrudan kaldırılır; tüketici uyarıyı nuget deprecate işareti (4.8.0 paketi, bakımcı) + geçiş rehberi + sürüm notuyla alır; politika sapması `docs/policies/deprecation.md`'ye bakımcı notu (S2) |
| D3 | Linux native paketi | (a) `Kck.Documents.SkiaSharp` yalnız `SkiaSharp`'a bağlı; Linux tüketicisi `SkiaSharp.NativeAssets.Linux.NoDependencies`'i kendi ekler (SkiaSharp resmi rehberi; 60 MB nupkg herkese indirilmez) · (b) Kck paketi NoDependencies'e bağlı (sıfır kurulum, her tüketici 60 MB indirir) | (a) + README/`docs/providers/documents.md` Linux kutusu + test projesi referansı (CI ubuntu) · **Karar: (a)** |

## 4. Kabul örnekleri (Kabul Kapısı adım 1 — testler bunlardan yazılır; 12 mevcut test taşınır)
- **Ö1 resize:** 200×100 PNG, `Width=100, Height=100, MaintainAspectRatio=true, OutputFormat="png"` → 100×50 PNG (imza `89 50 4E 47`).
- **Ö2 esnet:** aynı kaynak, `MaintainAspectRatio=false, 50×50` → 50×50.
- **Ö3 dönüştür:** PNG → `"webp"` (Quality 85) → WebP imzası (`RIFF….WEBP`), boyut korunur; `"jpg"`/`"jpeg"`/`"JPEG"` → `FF D8`.
- **Ö4 boş format:** `ConvertFormatAsync(png, "")` → PNG kalır; TIFF kaynak + boş format → JPEG (mevcut sözleşme).
- **Ö5 desteklenmeyen:** `"avif"`, `"unknown-xyz"` ve (D1a) `"gif"`, `"bmp"` → `NotSupportedException`, mesaj desteklenenleri listeler
  (`jpeg (jpg), png, webp`).
- **Ö6 girdi formatları:** GIF ve BMP **girdi** okunur ve PNG'ye dönüştürülür.
- **Ö7 boyut:** 320×240 PNG `GetDimensionsAsync` → (320, 240); 8000×8000 PNG başlığı (küçük dosya) → boyut okunur, piksel decode edilmez
  (`SKCodec.Create(stream).Info`; süre/bellek testi veya tasarım notu).
- **Ö8 saydamlık:** saydam PNG → JPEG → beyaz arka plan (belgeli; ImageSharp ile davranış farkı varsa CHANGELOG'da).
- **Ö9 EXIF:** yönlendirme 6 (90°) JPEG → resize sonucu yön uygulanmış (ImageSharp davranışıyla eşitlik testi; farklıysa belge).
- **Ö10 Linux CI:** `ubuntu-latest` üzerinde `Kck.Documents.SkiaSharp.Tests` geçer (`NativeAssets.Linux.NoDependencies` test projesinde).
- **Ö11 anahtar yok:** hiçbir `SixLaborsLicenseKey`/`sixlabors.lic` olmadan `dotnet build -c Release` 0 hata; `grep -ri sixlabors` →
  yalnız `CHANGELOG.md`, `decisions/`, `docs/adr/0028`, `docs/migrations/v3-to-v4.md` (tarihsel).
- **Ö12 lisans kapısı:** `license_gate.py` istisnasız geçer (SkiaSharp MIT); rapor "exception" satırında yalnız LGPL girdileri.
- **Ö13 AOT:** `samples/Kck.AotSmoke` yeni sağlayıcıyı içerir, native publish IL2104 vermez (P/Invoke tabanlı) — gerekirse "AOT-uyumlu ilan" notu.
- **Ö14 geçiş:** 5.0.0'da `Kck.Documents.ImageSharp` paketi yok; `docs/migrations/v4-to-v5.md` bölümü README snippet CI'ında derlenir;
  5.0.0 yayını sonrası nuget sayfasında eski paket "deprecated, use Kck.Documents.SkiaSharp" (S5, bakımcı).

## 5. Hamleler
| # | Hamle | Dosyalar | Kanıt | Bağımlılık | Geri alma | Kademe |
|---|---|---|---|---|---|---|
| S0 | **Yapıldı 2026-10-31:** kararlar D1a, D2b, D3a (sohbet); `decisions/…image-library-replacement.md` durumu "Onaylandı" | decisions/ | kullanıcı yanıtı | — | — | — |
| S1 | **Yeni sağlayıcı** `src/Providers/Kck.Documents.SkiaSharp`: `SkiaSharpImageProcessor : IImageProcessor` (`SKCodec` ile format tespiti + boyut, `SKBitmap.Resize`/`SKImage.Encode`, `SKSamplingOptions` kaliteli küçültme, EXIF yön düzeltme, saydam→JPEG arka plan), `AddKckDocumentsSkiaSharp()`, README.nuget.md (format matrisi + Linux kutusu), `PublicAPI` yok (provider); CPM `SkiaSharp 4.154.0` (+ test projesinde `SkiaSharp.NativeAssets.Linux.NoDependencies`); `tests/Kck.Documents.SkiaSharp.Tests` (Ö1–Ö9, 12 mevcut test taşınır + yeni); `stryker-documents-skiasharp.json` + matris; `Kck.ReadmeSnippets` snippet; sln; `docs/providers/documents.md` SkiaSharp bölümü; kök README tablosu; `docs-english.txt`; CHANGELOG `[Unreleased] Added`; `/currency-check` kapandı (artefakt) | 20+ dosya | RED→GREEN testler; Release build; `restore --locked-mode`; lisans kapısı | S0 | PR revert | K3 (yeni bağımlılık) |
| S2 | **Geçiş belgesi** (tek sürüm: kaynakta `[Obsolete]` ara adımı yok): `docs/migrations/v4-to-v5.md` "ImageSharp → SkiaSharp" bölümü (paket adı, `AddKckDocumentsSkiaSharp()`, gif/bmp çıktı farkı, Linux native notu); `docs/policies/deprecation.md`'ye bakımcı notu (deprecation sürümü olmadan kaldırma, gerekçe: lisans kısıtı); CHANGELOG `[Unreleased] Removed` + `Added` | 3 dosya | docs CI (dil, bağlantı, snippet derlemesi) yeşil | S1 | revert | K1 |
| S3 | ~~4.9.0 yayını~~ **kaldırıldı** (ara sürüm yok). Yerine: 5.0.0 yayını sonrası nuget.org'da `Kck.Documents.ImageSharp` 4.x paketleri **deprecate** işareti (alternatif: `Kck.Documents.SkiaSharp`) — **Kırmızı: bakımcı yapar** (S5'e taşındı) | nuget | deprecate görünür | S4 | — | — |
| S4 | **Kaldırma (5.0.0 — tek sürüm)**: proje + test + stryker + ReadmeSnippets referansı silinir; CPM satırı; `Directory.Build.props` lisans bloğu; `.gitignore sixlabors.lic`; 8 workflow env + yorumlar; `dependabot-lockfile.yml` yorumu; `license-policy.json` istisnası; CONTRIBUTING notu; `docs/licensing.md` satırları; known-issues K1 kapanış; `decisions/2026-10-14-…` kapanış notu; CHANGELOG `Removed`; solution-wide `restore --force-evaluate` + `--locked-mode` | ~25 dosya | Ö11, Ö12; CI tüm workflow'lar yeşil **secret olmadan** | S3 + v5 diğer kaldırmalar | revert | K3 (CI/secret) |
| S5 | **Bakımcı adımları (Kırmızı bölge, 5.0.0 yayını sonrası):** GitHub secret `SIXLABORS_LICENSE_KEY` sil (Actions + Dependabot); yerel `sixlabors.lic` sil; nuget.org'da eski `Kck.Documents.ImageSharp` paketlerini deprecate işaretle; Six Labors Community lisansını iptal/yenilememe (hesap kararı) | GitHub ayarları, nuget.org, yerel disk | `gh secret list` boş; CI yeşil; nuget sayfasında deprecate | S4 + yayın | yeniden ekleme | Kırmızı |

Sıradaki 3 hamle: dosyaların kabulü + commit → S1 (yeni sağlayıcı PR; TDD: önce 12 test SkiaSharp'a port + RED) → S2 (geçiş belgesi) → S4
(kaldırma, aynı sürüm dalgasında). Sıralama kardeş planla: F0 PR'ı S1'den önce `main`'e girer; **yayın yalnız en sonda, tek v5.0.0**.

## 6. Tehdit modeli · bitiş tanımı · bütçe
- **Kapsam içi:** (1) güvenilmeyen görüntü girdisi — decoder bombası/çok büyük boyut: `SKCodec.Info` ile piksel sayısı sınırı
  (`SkiaSharpOptions.MaxPixels`, varsayılan 50 MP) decode'dan önce; bozuk dosya → `SKCodec` null → `InvalidDataException`;
  (2) native kütüphane yükleme hatası Linux konteynerinde (`DllNotFoundException: libSkiaSharp`) → belge + test; (3) tedarik zinciri:
  SkiaSharp nuget paket kimliği doğrulandı (mono/SkiaSharp, MIT), SHA-sabit CI değişmez; (4) CI'da secret kaldırılırken fork PR'ları
  zaten anahtarsızdı → katkıcı deneyimi iyileşir.
- **Kapsam dışı:** animasyonlu GIF çıktısı (girdi ilk kare), ICC/renk profili yönetimi, AVIF/HEIF, görüntü üretimi (çizim).
- **Bitiş tanımı:** Ö1–Ö14 geçer; depo ve CI'da SixLabors kablolaması yok (Ö11); lisans kapısı istisnasız (Ö12); **tek 5.0.0** nuget'te
  doğrulanmış (kardeş planla aynı yayın); bakımcı S5'i tamamladı.
- **Tur bütçesi:** S1 ≤ 2 gün (`/code-review` + `/adversarial-review` güvenilmeyen girdi, en fazla 2 tur), S2 ≤ 0,5 gün, S4 ≤ 1 gün.

## 7. Etki haritası
- Değişen: §2 tablosu + yeni `src/Providers/Kck.Documents.SkiaSharp`, `tests/Kck.Documents.SkiaSharp.Tests`.
- Referans verenler: `Kck.Documents.Abstractions` (`IImageProcessor`, `ImageProcessingOptions` — **değişmez**), `samples/Kck.ReadmeSnippets`,
  `docs/providers/documents.md`, kök README, `docs/licensing.md`.
- Tutarlı kalacak kurallar: `docs/policies/deprecation.md` (D2b sapması bakımcı kararıyla kaydedilir), ADR-0009 (TryAdd), ADR-0022 (singleton),
  `.github/license-policy.json` izinli liste (MIT), `decisions/2026-10-26-license-gate.md`, CLAUDE.md lock-file kuralı.
- Koruyan testler: yeni test projesi, `readme_snippets.py` (README blokları), lisans kapısı testleri (`test_license_gate.py`), AOT smoke,
  template-smoke (şablon ImageSharp kullanmıyor — etkilenmez).

## 8. Riskler
| Risk | Olasılık | Önlem |
|---|---|---|
| Tüketici `gif`/`bmp` çıktısına bağımlı | Düşük | geçiş rehberi + sürüm notu (deprecation sürümü yok); D1c ile BMP geri eklenebilir |
| ImageSharp tüketicisi derleme-zamanı uyarısı almadan 5.0.0'da kırılır (tek sürüm) | Orta | nuget deprecate işareti (4.x paketleri), geçiş rehberi, CHANGELOG `Removed`, README "Breaking" kutusu |
| Linux tüketicisinde native yüklenmez | Orta | README/docs Linux kutusu (`NoDependencies`, Alpine musl), DI kaydında ilk kullanımda anlaşılır hata |
| Kalite/boyut farkları (WebP `quality==100` SkiaSharp'ta lossless; JPEG chroma varsayılanları) | Orta | eşitlik testleri belge notuna dönüşür; `Quality` sözleşmesi "kodlayıcı önerisi" |
| EXIF yön / saydamlık davranış farkı | Orta | Ö8/Ö9 testleri; fark CHANGELOG `Changed` |
| v5.0 kapsamı büyür, SixLabors kaldırma gecikir | Orta | tek sürüm kararı: yayın tüm fazlar bitince; v5 listesi `docs/migrations/v4-to-v5.md`'de kapanır; S1/S4 F1–F3'e paralel yürür |
| Secret silinmeden S4 birleşirse CI kırılmaz (env artık okunmuyor) — tersine S4 öncesi silinirse Release CI düşer | Düşük | sıra S4 → S5 |

## 9. Sınırlar
- Diğer LGPL bağımlılıklar (Hangfire, sonarscanner) ücretsizdir; kaldırma bu planın dışında (ayrı karar; Quartz zaten varsayılan).
- `ImageProcessingOptions` genişletmeleri (crop, watermark) kapsam dışı.

## 10. Kaynaklar
- Karar artefaktı ve kaynak listesi: `decisions/2026-10-31-image-library-replacement.md` (erişim 2026-10-31).
- SkiaSharp paket rehberi: https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md · encode kapsamı:
  https://github.com/mono/SkiaSharp/blob/main/documentation/docfx/guides/bitmaps/saving.md
- Deprecation politikası: `docs/policies/deprecation.md`; ImageSharp 4 kararı: `decisions/2026-10-14-sdk-10.0.402-imagesharp-4.md`.
