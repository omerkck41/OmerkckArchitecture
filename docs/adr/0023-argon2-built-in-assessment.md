# ADR-0023 — Argon2 Built-in Assessment

**Status:** Accepted  
**Date:** 2026-05-17  
**Deciders:** @omerkck41  
**Review Trigger:** .NET 11 roadmap / `System.Security.Cryptography.Argon2` ship tarihi

---

## Context

`Kck.Security.Argon2` provider'ı şu an `Konscious.Security.Cryptography.Argon2`
(v1.3.1) kullanıyor. Bu üçüncü taraf bağımlılığın yerine `.NET built-in` bir Argon2
implementasyonu geçilebilir mi sorusu gündeme geldi.

ADR-0001 (2026-04-25) Konscious.Argon2'yi ADO net-core/bcrypt yerine seçmişti;
bu ADR o kararın built-in alternatif bağlamında yeniden değerlendirilmesidir.

---

## Araştırma Bulguları

### .NET 10 / 9 Durumu

`System.Security.Cryptography` ad alanında **Argon2id built-in mevcut değildir**.

| .NET Sürümü | Durum |
|---|---|
| .NET 9 | Yok |
| .NET 10 | Yok |
| .NET 11 (roadmap) | Araştırılıyor — kesinleşmedi |

GitHub issue: [dotnet/runtime #26379](https://github.com/dotnet/runtime/issues/26379)
Son yorum (Şubat 2025): milestone atanmamış, aktif tartışma devam ediyor.

### Konscious.Argon2 v1.3.1 Değerlendirmesi

| Kriter | Değerlendirme |
|---|---|
| Bakım durumu | Aktif (son commit 2024) |
| OWASP uyumu | ✓ Argon2id, önerilen parametreler |
| Timing-safe karşılaştırma | ✓ `CryptographicOperations.FixedTimeEquals` |
| Güvenlik açığı (CVE) | Mevcut taramada yok |
| .NET 10 uyumu | ✓ Çalışıyor |
| Test kapsamı | Yeterli (kütüphane kendi testlerini içeriyor) |
| Bağımlılık ağacı | Sıfır transitif bağımlılık |

### Alternatif Değerlendirmesi

| Seçenek | Neden Elendi |
|---|---|
| `libsodium-net` / `Sodium.Core` | İkincil sistem kütüphanesi gerektirir; container deployment'ta risk |
| `BCrypt.Net-Next` | Argon2 değil, bcrypt — OWASP 2023+ tercihi Argon2id |
| Elle `CryptographicOperations` + PBKDF2 | Argon2id değil; GPU saldırısına karşı daha zayıf |
| .NET built-in (bekleme) | Şu an mevcut değil; blocker |

---

## Decision

**Konscious.Argon2 v1.3.1 korunur.**

Gerekçe:
1. `.NET 10`'da built-in Argon2 yoktur — geçiş teknik olarak imkânsız.
2. Konscious.Argon2 güvenlik, bakım ve performans kriterlerini karşılıyor.
3. Migration riski sıfır fayda için yüksek — ADR-0001 kararı geçerlidir.

---

## Review Tetikleyicisi

Aşağıdaki koşullardan **biri** gerçekleştiğinde bu ADR revize edilmeli:

1. `System.Security.Cryptography.Argon2` .NET resmi release'e girer.
2. Konscious.Argon2 bakımı sonlanır / CVE bulunur.
3. Proje .NET 11+'a geçer ve built-in Argon2 o sürümde ship edilmişse.

Kontrol: Her `Directory.Packages.props` güncelleme PR'ında Konscious paket sürümü
ve CVE taraması (`dotnet list package --vulnerable`) otomatik yapılır (build-test.yml).

---

## Consequences

**Pozitif:**
- Sıfır kod değişikliği, sıfır migration riski.
- OWASP Argon2id uyumu korunuyor.

**Negatif:**
- Üçüncü taraf bağımlılık sürmekte. Konscious.Argon2 bakımı sonlanırsa hızlı aksiyon gerekir.

---

## İlgili ADR'lar

- ADR-0001: Argon2 implementasyon seçimi (orijinal karar)
