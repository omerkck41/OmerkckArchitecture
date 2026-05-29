# Karar: FluentAssertions 8.x ticari lisansından AwesomeAssertions'a geçiş

- **Tarih:** 2026-05-29
- **Bağlam:** `dotnet list package --deprecated/--outdated` taramasında FluentAssertions 8.3.0
  tespit edildi. FluentAssertions v8 (Ocak 2025) Apache 2.0'dan Xceed ticari lisansına geçti
  ($130/geliştirici/yıl, ticari kullanımda zorunlu). Global kural "ücretsiz araç kullan,
  onaysız ücretli kurma" ile çelişiyor. 39 test projesi ve 94 kaynak dosya etkileniyor.
- **Adaylar:** AwesomeAssertions, Shouldly, FluentAssertions v7'ye pin, built-in/xunit Assert
- **Seçim:** AwesomeAssertions 9.4.0
- **Sürüm:** 9.4.0 (yayın: 2026-02-18)
- **Gerekçe:**
  - Lisans: Apache 2.0, "asla değişmeyecek, MIT'ye bile" taahhüdü → ücretsiz, kalıcı
  - Drop-in (API): tip adları ve assertion API'si FluentAssertions ile birebir aynı →
    test mantığı değişmedi, 94 dosyada sıfır assertion düzenlemesi
  - ⚠️ DÜZELTME (Doktrin #7): AwesomeAssertions **v9'da namespace'i yeniden adlandırdı**
    (`FluentAssertions` → `AwesomeAssertions`). Dolayısıyla "namespace korunur / sıfır-kod"
    iddiası YALNIZ ≤8.x için geçerliydi. 9.4.0 migrasyonu mekanik bir `using` değişimi gerektirdi:
    `using FluentAssertions;` → `using AwesomeAssertions;` (94 dosya, alt-namespace yok)
  - Güncel: 9.4.0 (2026-02-18), aktif geliştirme (4561 commit), net6/net8/net10 destekli
  - Shouldly elendi: farklı API (`.ShouldBe()`) → 94 dosyada tüm assertion'ların elle yeniden
    yazılması gerekir, yüksek migration maliyeti + regresyon riski
  - FA v7 pin elendi: sürüm dondurma teknik borç yaratır, v8'e kazara yükseltme riski, gelişim durur
- **Kaynaklar:**
  - https://www.infoq.com/news/2025/01/fluent-assertions-v8-license/ (erişim: 2026-05-29) — v8 Apache 2.0'ı terk etti
  - https://xceed.com/fluent-assertions-faq/ (erişim: 2026-05-29) — ticari lisans, $130/dev/yıl
  - https://github.com/AwesomeAssertions/AwesomeAssertions (erişim: 2026-05-29) — Apache 2.0 fork, 9.4.0, net10.0
  - https://www.conradakunga.com/blog/awesomeassertions-drop-in-replacement-for-fluentassertions/ (erişim: 2026-05-29) — namespace/tip adları korunur, drop-in
- **EOL:** N/A (FA v7 kritik fix alır ama gelişmez; AwesomeAssertions aktif sürüm hattı)
- **Migration adımları (uygulandı):**
  1. `Directory.Packages.props`: `FluentAssertions 8.3.0` → `AwesomeAssertions 9.4.0`
  2. 39 test `.csproj`: `PackageReference Include="FluentAssertions"` → `Include="AwesomeAssertions"`
  3. 94 `.cs`: `using FluentAssertions;` → `using AwesomeAssertions;` (v9 namespace rename)
  4. `dotnet restore` → build (0 hata) → test: tüm assertion testleri geçti; yalnız Docker
     gerektiren entegrasyon testleri (Persistence EF / Redis Testcontainers) lokalde Docker
     kapalı olduğu için atladı — migrasyonla ilgisiz, CI'da Docker mevcut
- **Post-mortem (Doktrin #7):** İlk doğrulamada Conrad Akunga blog'u (≤8.x dönemi) "namespace
  korunur" diyordu; bu AwesomeAssertions 9.x için geçersizdi. Ders: drop-in/uyumluluk iddiaları
  HEDEFLENEN sürüme göre teyit edilmeli — kaynak tarihi ≠ aday sürümü. Build CS0246 ile yakalandı,
  resmi v9 upgrade rehberiyle doğrulanıp düzeltildi.
- **Pre-commitment:** Adaylar araştırma öncesi beyan edildi; AwesomeAssertions doğrudan
  seçilmedi, Shouldly + v7-pin + built-in Assert ile kıyaslandı (Anti-Reflex #2)
- **İlgili:** xunit 2.9.3 "Legacy" (deprecated) → xunit.v3 ayrı/daha büyük migration (test projesi
  EXE'ye döner) olarak ERTELENDİ; bu kararın kapsamı dışında.
