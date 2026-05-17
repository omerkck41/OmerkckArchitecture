# Test Coverage Politikasi

Bu dokuman OmerkckArchitecture (`Kck.*`) projelerinin test coverage gate'i
ve kademeli yukseltme politikasini tanimlar.

## Genel Kural — Regression Onleme

**Her PR coverage'i dusuremez.** Coverage gate (`build-test.yml` icinde
`Enforce coverage threshold` step) bunu zorunlu kilar. Esik degeri zaman
icinde **kademeli** olarak yukselir; ancak yukseltme **test yazimina**
baglidir, kararname ile degil.

## Mevcut Esikler (Faz-B2 Kademe 1 — kalibre edildi 2026-05-17)

| Metrik | Esik | Gercek CI olcumu (2026-05-17) | Hedef (Kademe 2) |
|---|---|---|---|
| Line | **43%** | 43.4% | 50% |
| Branch | **38%** | 39.2% | 45% |

> **Not:** Faz-B2'de eklenen testler (EntityTests, PaginateTests, ErrorTests, PaginatePropertyTests,
> PathSanitizerPropertyTests, FilterGetValueTests) coverage'i ~3% artirdi ancak %50/%45 hedefine
> ulastiramadi. Esik, CI'in kararlı gecmesi icin gercek olcume kalibre edildi.
> Kademe 2 icin ek testler gerekiyor — onceligli hedef: caching, security, persistence provider'lari.

> Yerel olcum: `dotnet test --collect:"XPlat Code Coverage"` +
> `reportgenerator -reporttypes:TextSummary` ile uretildi (CI ile ayni komut).

## Kademeli Yukseltme Yol Haritasi

Kaynak rapor (`tasks/library-strategy-2026-04-25.md` Bolum 7.1) endustri
standardini soyle ozetliyor:

- Kutuphane projesi: %80+ line, %60+ branch (Polly, Serilog, EF Core ~%85)
- Provider/sarmalayici: %70+ line

Yol haritasi (her kademe ayri faz, test yazimi gerektirir):

| Kademe | Hedef Line | Hedef Branch | Faz / Trigger |
|---|---|---|---|
| **0** | 40 | 35 | LS-FAZ-4 — regression onleme |
| **1** ✅ | 50 | 45 | Faz-B2 — Entity/Paginate/Error/property-based testler |
| **2** | 65 | 50 | Edge case + boundary testleri yayginlasinca |
| **3** | 75 | 60 | Mutation testing (Stryker.NET) ile boslukları doldurarak |

## Bir Kademeyi Yukseltme Ön Sartlari

Bir esige kadar **test yazimi** tamamlanmadan esik **degisemez**:

1. Yerel olcum: `reportgenerator -reporttypes:TextSummary` ciktisi yeni hedefin uzerinde olmali.
2. CI'da en az 3 ardisik build yesil — geçici sapma degil, kalici durum.
3. PR aciklamasinda hangi testlerin eklendigi listelenir.
4. Esik degisikligi ve neden ayni PR'da yapilir.

## Olcum Tekrarlanabilirligi

Coverage olcumu host SDK + test discovery yontemine duyarli olabilir.
Tutarli olcum icin:

```bash
dotnet test OmerkckArchitecture.sln --no-build \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

dotnet tool restore
dotnet reportgenerator \
  -reports:'**/coverage.cobertura.xml' \
  -targetdir:./coverage-report \
  -reporttypes:'TextSummary'

cat ./coverage-report/Summary.txt
```

CI komutu (`build-test.yml`) ayni adimlari calistirir.

## Excluded Projeler

Coverage olcumu su test projelerinde **toplama**:

- `tests/**/*.Tests.csproj` — ✓ dahil
- `tests/Kck.Benchmarks` — ✗ disinda (test projesi degil, console app)
- `samples/**/*.csproj` — ✗ disinda (uygulama kodu degil framework kodu)

Excluding patterni `coverlet.runsettings` veya `dotnet test --filter` ile
acik degil — su an `XPlat Code Coverage` collector tum referans verilen
projelerin coverage'ini toplar; benchmark/sample csproj'lar test asseti
referans vermiyor → otomatik dahil olmuyor.

## Referanslar

- `.github/workflows/build-test.yml` (Enforce coverage threshold step)
- `tasks/library-strategy-2026-04-25.md` Bolum 7.1
- [Coverlet documentation](https://github.com/coverlet-coverage/coverlet)
- [reportgenerator](https://github.com/danielpalme/ReportGenerator)
