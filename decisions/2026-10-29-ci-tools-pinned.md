# Karar: docfx ve dotnet-sonarscanner yerel araç olarak sabitlenir

- Tarih: 2026-10-29
- Bağlam: `docs.yml` docfx'i, `sonar.yml` dotnet-sonarscanner'ı `dotnet tool install --global` ile sürüm belirtmeden
  kuruyordu: her koşu o günkü en son sürümü alıyor (tekrarlanabilir değil) ve lisans kapısı bu araçları görmüyordu
  (kapı yalnız `packages.lock.json` ve `dotnet-tools.json` manifestlerini okur). Kaynak: lisans kapısı adversarial
  review'u (PR #228).
- Pre-commitment: aday = CI'ın bugün fiilen kurduğu sürüm, nuget.org'daki en son kararlı sürümse; kaynak nuget.org
  katalog kaydı (lisans, yayın tarihi, deprecation, bilinen açık); eleme: registry kaydı, deprecation/açık, ücretli
  lisans.
- Adaylar:
  - A — İkisini `.config/dotnet-tools.json`'a sabitlemek, iş akışlarında `dotnet tool restore` + `dotnet docfx` /
    `dotnet sonarscanner`: sürüm tekrarlanabilir, lisans kapısı denetler, Dependabot'un `nuget` ekosistemi manifesti
    günceller. Seç.
  - B — Global kurulumda `--version` ile sabitlemek: tekrarlanabilir ama lisans kapısının dışında kalır. Ele.
- Seçim: A (baş mühendis, kullanıcı "taşı" dedi, 2026-10-29).
- Sürümler ve doğrulama (nuget.org katalog, erişim 2026-10-29):
  - docfx 2.81.0 — en son kararlı, yayın 2026-09-25, lisans MIT (SPDX), deprecation yok, bilinen açık yok. CI
    (docs run 40928278762) aynı sürümü kuruyordu.
  - dotnet-sonarscanner 12.0.0 — en son kararlı, yayın 2026-10-15, lisans dosyası `licenses/LICENSE.txt` = GNU LGPL
    v3, deprecation yok, bilinen açık yok. CI (sonar run, 2026-10-29) aynı sürümü kuruyordu.
  - `rules/deprecated-registry.md`: kayıt yok.
- EOL: CI araçları; endoflife.date'te kayıt yok. Güncellemeyi Dependabot önerir, lisans kapısı her sürümü denetler.
- Lisans: sonarscanner LGPL-3.0 → `.github/license-policy.json`'da belgelenmiş istisna (yalnız CI'da çalışır, hiçbir
  Kck paketine girmez/bağlanmaz; ücretsiz). Lisans metni SHA-256 ile sabit; metin değişirse kapı düşer.
- Etki: `docs.yml`, `sonar.yml`; yerel geliştirici `dotnet tool restore` ile aynı sürümleri alır.
