# Security Policy

Security policy for OmerkckArchitecture (the Kck.* package family).

## Supported Versions

This library is at version `v3.x`. Security patches are provided for the following versions:

| Version         | Supported           |
| --------------- | ------------------- |
| 3.x (latest)    | YES ✅              |
| 2.x             | Security-only ⚠️   |
| < 2.0           | NO ❌               |

## Reporting a Vulnerability

Please **do not** open a public GitHub issue. Instead:

- **Preferred:** GitHub Private Vulnerability Reporting
  (`Security` tab → `Report a vulnerability`)
- Email: omer_kck@msn.com

Include in your report:
- Affected package name and version
- Reproduction steps (a minimal repro, if possible)
- Impact: information disclosure, RCE, DoS, and so on
- Suggested fix (if any)

### Response Time

| Issue severity       | First response | Patch release    |
| -------------------- | -------------- | ---------------- |
| Critical (CVSS 9.0+) | 48 hours       | 7 days           |
| High (CVSS 7.0-8.9)  | 5 business days | 30 days         |
| Medium (CVSS 4.0-6.9)| 10 business days | 90 days        |
| Low (CVSS < 4.0)     | 30 days        | next release     |

## Disclosure Policy

Coordinated disclosure, **90 days** default embargo.
At the end of the embargo, a CVE will be requested and announced in `SECURITY-ADVISORIES.md`.

## Security-Related Links

- [Dependabot Alerts](https://github.com/omerkck41/OmerkckArchitecture/security/dependabot)
- [Code Scanning](https://github.com/omerkck41/OmerkckArchitecture/security/code-scanning)
- `SECURITY-ADVISORIES.md` (not a link yet: the file is created on the day it is published)
