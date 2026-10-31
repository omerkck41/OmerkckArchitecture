# Karar: CodeQL'de üretilmiş kod (obj/) sonuçları yüklemeden önce SARIF'ten çıkarılır

- Tarih: 2026-10-31
- Bağlam: Code scanning'de 22 açık uyarının 19'u `obj/` altındaki kaynak üretici çıktısındaydı (System.Text.Json
  bağlamları, `Microsoft.AspNetCore.OpenApi` XML yorum üreticisi). `.github/codeql-config.yml` `paths-ignore:
  **/obj/**` içeriyordu ama etkisizdi. Kullanıcı isteği: açık uyarıları kapat.
- Pre-commitment: ölçütler — resmi kaynak (tarihli), üretilmiş kodun dışarıda kalması garanti, analiz doğruluğu
  düşmesin, yeni üçüncü taraf action yok, birinci taraf koddaki bulgular susturulmasın.
- Kanıt (erişim 2026-10-31):
  - https://docs.github.com/en/code-security/code-scanning/creating-an-advanced-setup-for-code-scanning/customizing-your-advanced-setup-for-code-scanning
    — `paths`/`paths-ignore` derlenen dillerde yalnız "without building the code" (C/C++, C#, Java, Rust) uygulanır.
  - https://docs.github.com/en/code-security/code-scanning/creating-an-advanced-setup-for-code-scanning/codeql-code-scanning-for-compiled-languages
    — `none` doğruluğu "Good", manual "Best"; `none`'da "Generated code not analyzed".
  - `github/codeql-action/analyze` v4.39.1 (9b32540) `upload: failure-only` — "for users post-processing the SARIF
    file before uploading it"; `upload-sarif` aynı sürümde.
- Adaylar:
  - A — `build-mode: none`: `paths-ignore` çalışır, üretilmiş kod analiz edilmez; doğruluk Best → Good düşer. Ele.
  - B — derlemeli analiz + `upload: failure-only` + depodaki `codeql_sarif_filter.py` (yalnız `obj/`/`bin/` yol
    segmentli sonuçları siler, birim testli) + aynı sabitlenmiş `upload-sarif`. Yeni bağımlılık yok. Seç.
  - C — `query-filters` ile kuralları kapatmak: birinci taraf koddaki aynı kural bulgularını da susturur. Ele.
  - D — uyarıları elle "dismiss": üretilmiş satır numaraları değişince yeniden açılır. Ele.
- `rules/deprecated-registry.md`: kayıt yok. Yeni teknoloji yok (mevcut sabitlenmiş action'ın belgelenmiş girdisi).
- Kalan 3 not (birinci taraf kod: `cs/linq/missed-where` ×2, `cs/missed-ternary-operator`) kodda düzeltildi.
