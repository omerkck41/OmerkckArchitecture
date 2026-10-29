# Karar: actions/checkout v6 → v7 ve actions/setup-dotnet v5 → v6 (Dependabot #119, #127)

- **Tarih:** 2026-10-14
- **Durum:** Uygulandı — #147 ile birleşti (2026-10-15; SHA sabitlemeli). Kayıt 2026-10-30'da eklendi (commit edilmemiş kalmıştı).
- **Kapsam:** 9 workflow, yalnız `uses:` etiketleri (8 job + 1 adım her action için)

## Kaynaklar (erişim 2026-10-14, GitHub Releases API)
| Action | Mevcut | Hedef | Yayın | Runtime (`action.yml runs.using`) |
|---|---|---|---|---|
| actions/checkout | v6 (v6.1.0, 2026-07-20) | v7 (v7.0.0 2026-06-18, v7.0.1 2026-07-20) | kararlı | node24 → node24 |
| actions/setup-dotnet | v5 (v5.4.0, 2026-06-26) | v6 (v6.0.0, 2026-07-16) | kararlı | node24 → node24 |

- https://github.com/actions/checkout/releases/tag/v7.0.0 , https://github.com/actions/checkout/releases/tag/v7.0.1
- https://github.com/actions/setup-dotnet/releases/tag/v6.0.0

## Kırıcı değişiklik değerlendirmesi
- **checkout v7:** `pull_request_target` ve `workflow_run` olaylarında fork PR checkout'u engellenir (#2454); modül ESM'e
  geçti. Bu repoda hiçbir workflow bu iki tetikleyiciyi kullanmıyor (`git grep` boş) → etkisiz. v7.0.1 yalnız
  düzeltmeler (girdi kaçışlama, branch adı kırpma).
- **setup-dotnet v6:** ESM'e geçiş + bağımlılık güncellemeleri (`@actions/cache` 6.2.0). Workflow'larda `cache:`
  girdisi kullanılmıyor; SDK sürümü `global.json` ile sabit (10.0.402) → davranış değişikliği beklenmiyor.
- Runtime her iki action'da da değişmedi (node24); runner gereksinimi aynı.

## Alternatifler
1. **Güncelle (seçilen):** kırıcı değişiklikler bu repoyu etkilemiyor; eski ana sürümler bakımdan düşecek.
2. Ertele: v6.1.0 / v5.4.0 hâlâ yamalı, ama Dependabot PR'ları açık kalır ve sapma büyür.
3. SHA ile sabitleme (zizmor bulgusu, ayrı iş): bu kararla çelişmez; sürüm yükseltmesinden sonra yapılabilir.

## Not
#119 / #127'deki eski CI hataları action değişikliğinden değil, eski base'den (`359bc6a`; SQLitePCLRaw 2.1.11,
NU1903) kaynaklanıyordu — #142 ile main'de kapandı. `@dependabot rebase` istendi.
