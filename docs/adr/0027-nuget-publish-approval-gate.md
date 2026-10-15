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

---

## Güncelleme — 2026-10-15: NuGet Trusted Publishing

nuget.org otomatik yayında uzun ömürlü API anahtarlarını önermiyor; GitHub Actions için Trusted Publishing destekleniyor.

**Karar:** Yayın job'u `NuGet/login` (v1.2.0, SHA'ya sabit) ile GitHub OIDC token'ını push'tan hemen önce **1 saat geçerli,
tek kullanımlık** bir API anahtarına çevirir. Kalıcı `NUGET_API_KEY` secret'ı kullanılmaz.

- nuget.org politikası (owner `omerkck`): Repository Owner `omerkck41` (#106805727) · Repository `OmerkckArchitecture`
  (#1219365314) · Workflow `nuget-publish.yml` · Environment `nuget` · Scope: push new packages and versions · Glob `Kck.*`.
  Unlist/relist yetkisi verilmedi.
- İki job (güvenlik incelemesi 2026-10-15: OIDC izni repo kodu çalıştıran adımlara açık olmamalı):
  `build` (doğrulama, derleme, test, paket, SBOM; yalnız `contents: read`, ortam yok, onay beklemez) →
  `publish` (`needs: build`; yalnız `id-token: write`, `nuget` ortamı + required reviewer; repo kodu çalıştırmaz:
  paket indir, .NET kur, OIDC girişi, push). Onaylayan, derleme ve testlerin geçtiğini gördükten sonra onaylar.
- `user` girdisi nuget.org profil adıdır (`omerkck`), GitHub kullanıcı adı değil.
- **Yalnız en yeni sürüm listede (bakımcı isteği, 2026-10-15):** publish job'u başarılı push'tan sonra, yayınlanan
  paketlerin yeni sürümden **daha düşük** bütün kararlı sürümlerini listeden kaldırır (`dotnet nuget delete` = nuget.org'da
  unlist; silinmez, tam sürümle restore edilir, tekrar listelenebilir). Ön sürüm yayını hiçbir şey kaldırmaz. Politikaya
  "Unlist or relist package versions" kapsamı bu yüzden eklendi. Kuru prova (gerçek nuget.org verisi): 3.5.0 için
  59 pakette 344 sürüm; 3.5.0'a dokunulmaz.
- **İlk başarılı yayından sonra:** `nuget` ortamındaki `NUGET_API_KEY` secret'ı silinir ve nuget.org'daki eski API anahtarı
  iptal edilir.

Kaynak: https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing (erişim 2026-10-15) ·
https://github.com/NuGet/login (v1.2.0, `8d196754b4036150537f80ac539e15c2f1028841`)
