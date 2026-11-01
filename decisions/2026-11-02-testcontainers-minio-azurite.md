# Karar: FileStorage davranış testleri için MinIO ve Azurite container'ları (F4b)

- Tarih: 2026-11-02
- Bağlam: `docs/plans/2026-10-31-provider-saglik-ve-atil-kod-plani.md` §4 F4 (FileStorage: "MinIO + Azurite Testcontainers"), Ö14/Ö15.
  `Kck.FileStorage.AwsS3` ve `Kck.FileStorage.AzureBlob` için bugün yalnız yol doğrulama birim testleri var; F0 K7 yol kaçışı testi
  gerçek Azurite'e karşı koşmuyor. Depo Testcontainers 4.16.0 modüllerini (PostgreSql, Redis, RabbitMq, Elasticsearch) zaten kullanıyor;
  F4a `DockerFact` ile Docker yoksa skip, CI'da zorunlu.
- Pre-commitment (araştırmadan önce): adaylar Testcontainers ailesinin resmi MinIO ve Azurite modülleri, mevcut 4.16.0 hattında;
  doğrulama kaynakları nuget.org flatcontainer + `gh api` (testcontainers/testcontainers-dotnet) + registry; eleme kriterleri: kimlik
  resmi kayıtta var mı, 4.16.0 hattıyla uyumlu mu, repo aktif/MIT mi, açık güvenlik bildirimi var mı, registry eşleşmesi.
- Registry taraması (Anayasa #6): `testcontainers`, `minio`, `azurite` → kayıt yok.

## Doğrulama (nuget.org flatcontainer + gh api, erişim 2026-11-02)

| Kimlik | En son kararlı | 4.16.0 var mı | Not |
|---|---|---|---|
| `Testcontainers` (çekirdek) | 4.16.0 | evet | modüllerin geçişli bağımlılığı; `ContainerBuilder` genel container |
| `Testcontainers.Azurite` | 4.16.0 | evet | resmi modül |
| `Testcontainers.Minio` | **4.15.0** | **hayır** | modül 4.16.0 ile yayınlanmamış; çekirdek 4.16.0 ile sürüm karışımı olur |
| `Testcontainers.Redis` / `.PostgreSql` (mevcut) | 4.16.0 | evet | referans |

- Repo `testcontainers/testcontainers-dotnet`: aktif (push 2026-10-29), sürüm 4.16.0 (2026-10-09), lisans **MIT**, açık güvenlik bildirimi 0.
- EOL: kütüphane EOL takvimi yok (N/A); .NET 10 LTS 2028-11-14.
- İmajlar: `mcr.microsoft.com/azure-storage/azurite` (Microsoft, MIT), `minio/minio` (AGPL-3.0 — yalnız test container'ı olarak çalıştırılır,
  pakete girmez, lisans kapısı NuGet paketlerini tarar; dağıtılan hiçbir şey MinIO içermez).

## Adaylar

| Aday | Karar |
|---|---|
| A — `Testcontainers.Azurite` 4.16.0 modülü | **Seçildi** (Blob testleri) |
| B — `Testcontainers.Minio` 4.15.0 modülü | Elendi: 4.16.0 hattında yok; çekirdekle sürüm karışımı; aynı işi C yapar |
| C — `Testcontainers` çekirdek 4.16.0 + genel `ContainerBuilder` ile `minio/minio` (port 9000, `MINIO_ROOT_USER/PASSWORD`, `/minio/health/ready` bekleme) | **Seçildi** (S3 testleri); tek sürüm hattı, ~15 satır fixture |
| D — `adobe/s3mock` genel container | Elendi: önceki probe P7'de kullanıldı ama MinIO plan kararı; S3Mock Java tabanlı, daha ağır |
| E — Gerçek bulut hesabı | Elendi: maliyet, secret, CI dışı |

## Düzeltme (2026-11-02, uygulama sırasında) — MinIO imajı çekilemiyor → S3Mock

- Uygulama: `docker pull minio/minio:RELEASE.2025-09-07T16-13-09Z` ve `:latest` → "pull access denied / repository does not exist";
  `quay.io/minio/minio` → 401. `docker manifest inspect minio/minio:latest` de başarısız (2026-11-02). Projenin kendi notu (probe P5/P7,
  2026-10-31: "MinIO imajı Docker Hub'da yok") karar yazılırken gözden kaçtı — post-mortem: imaj seçiminde önceki probe notları ve
  `docker manifest inspect` ön kontrol zorunlu (ders önerisi ekosisteme).
- Yeni aday: **`adobe/s3mock:5.2.3`** (genel `ContainerBuilder`, port 9090, log bekleme). Doğrulama (2026-11-02): `docker manifest inspect`
  başarılı (Docker Hub'da var, yerelde de cache'li); GitHub `adobe/S3Mock` aktif (push 2026-10-30), en son sürüm 5.2.3 (2026-09-19), lisans
  **Apache-2.0**, açık güvenlik bildirimi 0. Probe P7 (2026-10-31) aynı imajla koştu. Elenen: MinIO (imaj yok), LocalStack (ağır, lisans
  karışık), gerçek S3 (maliyet/secret).
- Sonuç: C adayı **S3Mock** ile uygulanır; paket kararı değişmez (çekirdek `Testcontainers` 4.16.0 + genel container). `docs/test-strategy.md`
  tablosu buna göre.

## Seçim

- CPM'e **iki** satır: `Testcontainers.Azurite` 4.16.0, `Testcontainers` 4.16.0 (çekirdek, AwsS3 testleri için doğrudan referans).
- `Kck.FileStorage.AwsS3.Tests`: **S3Mock** fixture (bkz. "Düzeltme"; `AwsS3Options.ServiceUrl` + `ForcePathStyle`, H6'da eklenmişti), roundtrip
  testleri `[DockerFact]`.
- `Kck.FileStorage.AzureBlob.Tests`: Azurite fixture (`UseDevelopmentStorage`/bağlantı dizesi), roundtrip testleri + K7 yol kaçışı testinin gerçek
  Azurite'e karşı koşan sürümü, `[DockerFact]`.
- Lock dosyaları: solution-wide `restore --force-evaluate` → `--locked-mode`.
- Kayıt: bu dosya; `docs/test-strategy.md` container listesi güncellenir. Testlerin bulduğu sağlayıcı defektleri (S3 boş sayfa NRE, kök listeleme,
  `Overwrite=false` sessiz üzerine yazma ve KeyPrefix'li `FullPath`; Blob kök listeleme ve 409 → IOException) CHANGELOG `Fixed`'e girer.
- Pre-commitment hash: bu dosyanın "Pre-commitment" paragrafı.
