# Karar: benchmark.yml — main'de fail-on-alert kapatılır (ölçüm gürültüsü, gerçek regresyon yok)

- **Tarih:** 2026-10-14
- **Durum:** ONAYLANDI (kullanıcı, 2026-10-14)
- **Bağlam:** `.github/workflows/benchmark.yml` (Performance Regression) Mayıs 2026'dan beri main'deki son 6
  push'un hepsinde kırmızı. Action: `benchmark-action/github-action-benchmark@v1`, `alert-threshold: '110%'`,
  `fail-on-alert: true`. İş yalnızca `push: main` ve `workflow_dispatch` ile çalışıyor — yani **merge sonrası**.

## Pre-commitment (araştırmadan önce beyan)
- **Ne arayacağım:** her alarmda oran, alarm veren benchmark, ölçülen kodun o aralıkta değişip değişmediği,
  runner donanımı; action'ın güncel girdileri (resmi `action.yml` + README).
- **Gerçek regresyon kriteri:** ölçülen kaynak dosya değişmiş olmalı **ve** artış sonraki çalışmalarda kalıcı olmalı
  (aynı donanımda). Değişmeyen kodda iki yönlü dalgalanma = gürültü.
- **Seçenek değerlendirme kriterleri:** (1) geçmiş 25 karşılaştırmada yanlış alarm sayısı, (2) gerçek regresyonu
  görünür tutma, (3) ücretsiz kalma (paid runner yok), (4) karmaşıklık/bakım, (5) CI süresi.

## Kanıt
Kaynak: `gh run view --log` (son 4 çalışma; Mayıs çalışmalarının logları silinmiş, HTTP 410) ve gh-pages
`dev/bench/data.js` (26 kayıt, 2026-05-24 → 2026-10-14; tüm geçmiş).

1. **Ölçülen kod değişmedi.** `src/Abstractions/Kck.Core.Abstractions/Results/Result.cs` ve `Paging/Paginate.cs`
   son olarak #69'da (v3.0) değişti — bu, benchmark geçmişinin tamamından önce. `359bc6a..73bc114` aralığında
   `tests/Kck.Benchmarks` altında yalnız `packages.lock.json` değişti.
2. **Runner donanımı çalışmadan çalışmaya değişiyor** (`ubuntu-latest`, aynı etiket):

   | Run | Commit | CPU | Medyan oran (önceki koşuya göre) |
   |---|---|---|---|
   | 38605978342 | eee76fb | AMD EPYC 9V45, x86-64-v4 (AVX-512) | 0.63 |
   | 38608693867 | a710530 | AMD EPYC 9V45 | 1.03 |
   | 38616230550 | f443a7b | AMD EPYC 7763, x86-64-v3 (AVX2) | **1.80** (12/12 benchmark alarm, 1.57x–3.41x) |
   | 38626763801 | 73bc114 | AMD EPYC 7763 | 1.04 (2 alarm: 1.117x, 1.119x) |

   f443a7b (ImageSharp yükseltmesi — benchmark edilen koda dokunmuyor) her benchmark'ı aynı anda 1.6–3.4x
   "yavaşlattı": tipik donanım geçişi imzası.
3. **İki yönlü dalgalanma.** 25 karşılaştırmanın medyan oranları 0.63–1.80 arasında; değişmeyen kodda tek tek
   benchmark'ların min/max aralığı 1.8x–11.1x. 17/25 karşılaştırma %110 eşiğini aşıyor.
4. **Alt-nanosaniye benchmark'lar anlamsız.** `ResultBenchmarks.Match_*` 0.10–1.64 ns (timer çözünürlüğünün altı,
   JIT inline'a duyarlı) → 4.3x/11.1x aralık, tekil alarmlar 5.10x / 6.79x.
5. **Eşik simülasyonu** (geçmiş 25 karşılaştırma, kaç çalışma kırmızı olurdu):

   | Eşik | 110% | 120% | 130% | 150% | 200% | 300% |
   |---|---|---|---|---|---|---|
   | Kırmızı çalışma | 17 | 12 | 11 | 8 | 4 | 4 |

   300%'de bile 4 çalışma kırmızı — mutlak, ardışık koşular arası karşılaştırma bu runner'larda güvenilir değil.
6. **Yan bulgu (hijyen):** merge adımı `*-report*.json` ile hem `JsonExporter.Brief` (Program.cs) hem
   `--exporters json` çıktısını (`-report-full-compressed.json`) alıyor → her benchmark gh-pages'e **iki kez**
   yazılıyor, alarm tabloları çift satır. Değerler aynı olduğundan alarmları tetiklemiyor, ama veri kirli.

**Teşhis:** Gerçek regresyon yok. Kırmızı çalışmalar, paylaşımlı runner'da heterojen donanım + %10 eşik +
ardışık tek koşu karşılaştırmasının sonucu.

## Action girdileri (resmi kaynak)
`benchmark-action/github-action-benchmark` en son sürüm **v1.22.2** (2026-09-15, `gh release list`).
`action.yml` (main, erişim 2026-10-14) ilgili girdiler: `alert-threshold` (default `200%`), `fail-on-alert`
(default `false`), `fail-threshold` (default = alert-threshold, ≥ alert-threshold olmalı), `comment-on-alert`,
`summary-always`, `max-items-in-chart`. Karşılaştırma **yalnız bir önceki kayıtla**; ortalama/baseline/
tekrar girdisi yok. benchmarkdotnet örneği `-report-full-compressed.json` dosyasını kullanmayı öneriyor
(`examples/benchmarkdotnet/README.md`).
- https://github.com/benchmark-action/github-action-benchmark/blob/master/action.yml
- https://github.com/benchmark-action/github-action-benchmark#fail-on-alert-optional
- https://github.com/benchmark-action/github-action-benchmark/tree/master/examples/benchmarkdotnet

## Alternatifler
| # | Seçenek | Yanlış alarm | Regresyon görünürlüğü | Maliyet | Sonuç |
|---|---|---|---|---|---|
| A | Eşiği yükselt (alert/fail-threshold 150–300%) | 8 → 4 / 25 hâlâ kırmızı | Düşer (%50 altı regresyon görünmez) | Yok | **Ret** — donanım geçişini çözmüyor |
| B | main'de `fail-on-alert: false`; alarm yorumu + job summary kalsın, yorum eşiği 150% | 0 kırmızı; 8/25 bilgi amaçlı yorum | Korunur (yorum + gh-pages grafiği) | Yok, 2 satır | **Seçilen** |
| C | Ölçüm tekrarını artır (`--launchCount`, iteration) | Varyansı azaltır, donanım kaymasını (medyan 1.8x) **azaltmaz** | Aynı | CI süresi ~8.5 dk → 2–3 katı | **Ret** |
| D | PR'da aynı runner'da A/B (base + head aynı işte, oran karşılaştırması) | Donanım etkisini yok eder | Gerçek kapı (merge öncesi) | Süre ~2x, özel script | **Ertelendi** — ayrı iş (aşağıda) |
| E | Sabit donanım (self-hosted / larger runner) | Çözer | Korunur | Ücretli / altyapı | **Ret** (ücretsiz araç kuralı) |

## Gerekçe (B)
- İş merge **sonrası** çalışıyor: kırmızı sonuç hiçbir regresyonu engellemiyor, yalnız main'i kırmızı gösteriyor ve
  gerçek CI kırmızılarını maskeliyor (alarm yorgunluğu). Kapı değeri ≈ 0, maliyeti gerçek.
- Veri kaybı yok: gh-pages geçmişi, grafik ve commit yorumu devam eder. Yorum eşiği 110% → 150%: geçmişte
  17 yerine 8 yorum (çoğu donanım geçişi) — sinyal/gürültü iyileşir.
- Aynı PR'da hijyen: merge glob'u yalnız `*-report-full-compressed.json` (action'ın önerdiği format) → çift kayıt biter.

## Değişiklik (onay sonrası)
```yaml
alert-threshold: '150%'
fail-on-alert: false
```
+ merge adımında `-name '*-report-full-compressed.json'`.

## Yeniden değerlendirme / takip
- **D (PR A/B karşılaştırması)** gerçek bir performans kapısı istenirse ayrı iş olarak ele alınır.
- `Match_*` alt-ns benchmark'ları anlamlı iş yükü taşıyacak şekilde yeniden tasarlanabilir (ayrı iş).
- **EOL:** github-action-benchmark aktif bakımda (v1.22.2, 2026-09-15); EOL ilanı yok (N/A).
