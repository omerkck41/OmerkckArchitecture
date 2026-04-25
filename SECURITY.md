# Security Policy

OmerkckArchitecture (Kck.* paket ailesi) icin guvenlik politikasi.

## Supported Versions

Bu kutuphane su anda `0.x` (pre-1.0) suruminde olup yalniz **en son minor**
versiyon icin guvenlik yamasi alir. v1.0 yayinlandiktan sonra en son iki
minor surumu (N, N-1) destekleyecegiz.

| Version                  | Supported |
| ------------------------ | --------- |
| 0.x.x (latest minor)     | YES       |
| 0.x.x (previous minors)  | NO        |
| < 0.1                    | NO        |

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
