# Security Policy

OmerkckArchitecture (Kck.* paket ailesi) icin guvenlik politikasi.

## Supported Versions

Bu kutuphane `v3.x` surumundedir. Guvenlik yamasi asagidaki surumler icin saglanir:

| Version         | Supported           |
| --------------- | ------------------- |
| 3.x (latest)    | YES ✅              |
| 2.x             | Security-only ⚠️   |
| < 2.0           | NO ❌               |

## Reporting a Vulnerability

Lutfen public GitHub issue **acmayin**. Bunun yerine:

- **Tercih edilen:** GitHub Private Vulnerability Reporting
  (`Security` sekmesi → `Report a vulnerability`)
- Email: omer_kck@msn.com

Raporunuza dahil edin:
- Etkilenen paket adi ve versiyonu
- Yeniden uretim adimlari (mumkunse minimal repro)
- Etki: bilgi ifsasi, RCE, DoS, vb.
- Onerilen duzeltme (varsa)

### Yanit Suresi

| Sorun siddeti        | Ilk yanit    | Patch yayini     |
| -------------------- | ------------ | ---------------- |
| Critical (CVSS 9.0+) | 48 saat      | 7 gun            |
| High (CVSS 7.0-8.9)  | 5 is gunu    | 30 gun           |
| Medium (CVSS 4.0-6.9)| 10 is gunu   | 90 gun           |
| Low (CVSS < 4.0)     | 30 gun       | sonraki release  |

## Disclosure Policy

Coordinated disclosure, **90 gun** default embargo.
Embargo sonunda CVE alinacak ve `SECURITY-ADVISORIES.md`'de duyurulacak.

## Security-Related Links

- [Dependabot Alerts](https://github.com/omerkck41/OmerkckArchitecture/security/dependabot)
- [Code Scanning](https://github.com/omerkck41/OmerkckArchitecture/security/code-scanning)
- [SECURITY-ADVISORIES.md](./SECURITY-ADVISORIES.md) (publish edildigi gun olusur)
