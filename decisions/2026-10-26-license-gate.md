# Karar: Lisans kapısı — depo içi, fail-closed betik

- **Tarih:** 2026-10-26
- **Durum:** Uygulandı (bakımcı kabulü bekliyor)
- **Kısıt:** projede ücretli lisans kullanılmaz (bakımcı, 2026-10-26).
- **Plan:** `docs/plans/2026-10-26-dis-inceleme-yol-haritasi.md` (F0 / H1-H2)

## Sorun
`license-audit.yml` lisans alanında yalnız GPL/LGPL/SSPL/BUSL dizgesi arıyordu. NuGet'te `<license type="file">` ile
gelen paketler hiç eşleşmiyordu: MediatR 14.1.0 (RPL-1.5 veya ticari), SixLabors.ImageSharp 4.1.3 (Split 1.0),
Hangfire 1.8.25 (LGPL-3.0). Araç hatası `|| true` ile yutuluyor (fail-open), `nuget-license` sürümsüz kuruluyordu.
2026-10-26 çalışması (run 40324570972) "OK: no copyleft contamination detected" verdi.

## Kriterler (araştırmadan önce)
fail-closed · dosya/URL lisanslarını çözebilme · lisans içeriği değişince yakalama · sürümsüz 3. parti araç yok ·
CI'ın zaten çözdüğü bağımlılık kümesiyle birebir aynı küme · tek bakımcı için düşük bakım.

## Alternatifler
| Seçenek | Değerlendirme | Sonuç |
|---|---|---|
| Depo içi betik: `packages.lock.json` + yerel nuspec + politika dosyası | Lock dosyaları CI'ın `--locked-mode` ile çözdüğü kümenin tamamı; dosya lisansları içerik SHA-256'sıyla sabitlenir, metin değişirse kapı düşer; ek araç yok. Bu oturumdaki tarama 3 sorunlu aileyi buldu | **Seçildi** |
| `nuget-license` 4.0.18 (nuget.org, 2026-10-26) sabit sürüm + allowlist + override | Olgun araç; ancak dosya lisanslı paketler için sürüm başına override gerekir, metin değişimini yakalamaz; yine 3. parti araç | Elendi (Plan B) |
| GitHub dependency-review-action | Yalnız PR'da çalışır; dosya lisans verisi NOASSERTION olabilir; zamanlanmış tarama yok | Elendi |

## Politika (bakımcı onayı D1, 2026-10-26)
- İzinli SPDX: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, MS-PL, ISC, PostgreSQL, Zlib, Unlicense, 0BSD;
  `OR` ifadesinde en az bir izinli seçenek yeterli.
- Belgeli istisnalar: Hangfire* (LGPL-3.0, dinamik bağlama), SixLabors.ImageSharp (Split 1.0; Kck tüketicisine geçişli
  Apache-2.0), MediatR (RPL-1.5/ticari — **vadeli, 2027-01-31**; v4.0.0'da paket kaldırılır).
- Vadesi geçen istisna, bilinmeyen lisans, okunamayan nuspec, lock dosyası yokluğu → kapı düşer.

## Kaynaklar (erişim 2026-10-26)
- https://api.nuget.org/v3-flatcontainer/nuget-license/index.json (son sürüm 4.0.18)
- Paket lisans metinleri: yerel NuGet önbelleği (MediatR 14.1.0 `LICENSE.md`, ImageSharp 4.1.3 `LICENSE`, Hangfire 1.8.25 `LICENSE.md`)
- https://github.com/actions/dependency-review-action
