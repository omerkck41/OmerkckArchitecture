# ADR-0027 — NuGet Yayını İçin Onay Kapısı (Korumalı Environment)

**Status:** Accepted  
**Date:** 2026-10-14  
**Deciders:** @omerkck41  
**Breaking:** Hayır (paket API'si değişmez; yalnız yayın süreci)

---

## Context

`nuget-publish.yml`, `v*.*.*` tag push'unda (veya `workflow_dispatch` ile) paketleri doğrudan nuget.org'a gönderiyor.
Arada insan onayı yok ve `NUGET_API_KEY` repo düzeyinde bir secret.

Sorunlar:
1. **Geri alınamazlık:** nuget.org paket silmeyi desteklemez, yalnız listeden kaldırır (unlist).
   Yanlış bir tag tüm tüketicilere ulaşan kalıcı bir sürüm üretir.
2. **Çok-ajanlı geliştirme:** Geliştirme Claude Code ajanlarıyla (yerel, worktree, bulut) yürüyor. Yerel izin
   kuralları her oturumda bulunmayabilir; yayın kapısı sunucu tarafında olmalı.
3. **Anahtar kapsamı:** Repo düzeyindeki secret her job tarafından okunabilir.

---

## Decision

Publish job'u korumalı `nuget` environment'ına bağlanır (`environment: nuget`).

**Environment ayarları (GitHub → Settings → Environments → `nuget`):**
- Required reviewers: `@omerkck41`
- Prevent self-review: **kapalı** — ajanlar bakımcının kimliğiyle push eder; açık olursa yayın onaylanamaz
- Deployment branches and tags: tag `v*.*.*` + branch `main` (`workflow_dispatch` main ref'inde çalışır)
- Secret: `NUGET_API_KEY` environment düzeyinde; repo düzeyindeki kopyası silinir

GitHub dokümanına göre environment secret'larına, zorunlu onaylayıcılardan biri onaylamadan job erişemez.
Free planda required reviewers public repolarda kullanılabilir.

**Sıra:** Önce environment ve secret kurulur, sonra bu workflow değişikliği merge edilir, en son repo secret'ı silinir.

**Mevcut durum (2026-10-14, salt-okunur API sorgusu):** repo public · `main` korumalı (zorunlu kontroller:
CodeQL `Analyze (csharp)`, `Build & Test` ubuntu + windows; strict; force push kapalı; adminler dahil) ·
`nuget` environment'ı henüz yok — merge'den önce oluşturulmalı.

### Kapsam ve kalan risk
- Kapı; kazaya ve aşırı istekli ajana karşıdır. Ajanlar bakımcının `gh`/`git` kimliğini taşıdığı için onay
  REST API ile de verilebilir. Bakımcının Claude Code ayarlarında şu çağrılar her seferinde onay ister:
  `pending_deployments`, `environments`, `protection`, `git/refs`, `gh secret` ve `gh release edit`.
- Bu kurallar betik dolaylaması gibi kasıtlı atlatmaya karşı garanti değildir. Daha güçlü seçenek: ajana ayrı,
  en az yetkili bir kimlik vermek (deployment onayı, yönetim ve secret yetkisi olmayan token). O zaman
  "prevent self-review" açılabilir. Bu ayrı bir karar konusudur; izin kapsamı dokümandan doğrulanacak.

### Değerlendirilen alternatifler
| Alternatif | Neden seçilmedi |
|---|---|
| Yalnız `workflow_dispatch` (tag tetikleyicisi kaldırılır) | Dispatch de insan onayı olmadan tetiklenebilir; kapı değil |
| Yalnız yerel ajan izin kuralları | Bulut/diğer makine oturumlarında yok; sunucu tarafı garanti vermez |
| NuGet trusted publishing (OIDC) | Uzun ömürlü anahtarı kaldırır ama onay eklemez; ayrı değerlendirme |

---

## Doğrulama

`workflow_dispatch` ile mevcut bir tag (ör. `v3.4.0`; `--skip-duplicate` var) tetiklenir → job
"Waiting for review" durumunda bekler → reddedilir → yayın yapılmaz.

---

## Consequences

**Pozitif:**
- Her yayın bakımcının GitHub'da bilinçli onayını gerektirir; ajanlar yayın yapamaz.
- API anahtarı onaysız job'a verilmez.

**Negatif:**
- Yayın başına bir onay tıklaması.
- Bakımcı erişilemezse yayın bekler (bilinçli tercih).

---

## Kaynaklar

- https://docs.github.com/en/actions/reference/workflows-and-actions/deployments-and-environments (erişim 2026-10-14)
- https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax — `jobs.<job_id>.environment`
- https://learn.microsoft.com/en-us/nuget/nuget-org/policies/deleting-packages — "nuget.org does not support permanent deletion of packages"

## İlgili

- `decisions/2026-10-14-claude-process-profile.md` (TIER 3)
