# OmerkckArchitecture v2 Dogrulama Duzeltmeleri — Plan

- Tarih: 2026-10-14 · Kademe: K3 (guvenlik kapisi, izin modeli, 2 repo) · Onay: kullanici 2026-10-14 ·
  Durum: **TAMAMLANDI (2026-10-15)** — PR #139–#148; arsiv: tasks/archive/2026-10-15-omerkck-v2-fixes.md
- Profil: `.claude` reposu → Standart (hook) · OmerkckArchitecture → Kurumsal (TIER 3, G0 onayina bagli)
- Girdi: v2 dogrulama raporu (bu oturum) + kullanici duzeltmesi: proje ekip olcekli (Claude cok-ajanli ekip,
  kullanici denetleyici/karar verici), hedef herkese fayda saglayan surdurulebilir kutuphane.
- Hedef: Bulunan aciklari risk sirasina gore kapatmak; Kabul Kapisi ve yayin kapisini metinden deterministik
  korumaya tasimak; projeye dogru profili (TIER 3) yazmak.
- Temel durum: hook 171/171 · ekosistem 197/197 (2026-10-14, degisiklik oncesi)

## Kaynaklar (Anayasa #1, #4 — erisim 2026-10-14)
- code.claude.com/docs/en/permissions: "Rules are evaluated in order: deny, then ask, then allow" · ask kurali auto
  modda da sorar · "`Bash(git log *)` matches `git log`" (sondaki ` *` yalin komutu da kapsar)
- code.claude.com/docs/en/memory: proje talimati `./CLAUDE.md` veya `./.claude/CLAUDE.md`; "a gitignored
  CLAUDE.local.md only exists in the worktree where you created it" · CLAUDE.md zorlama degil baglamdir → PreToolUse
- docs.github.com (deployments-and-environments): Free/Pro/Team'de required reviewers public repoda var ·
  "a job cannot access environment secrets until one of the required reviewers approves it" · prevent self-review
- docs.github.com (workflow-syntax): `environment: <ad>` (job duzeyi)
- learn.microsoft.com/nuget (deleting-packages): "nuget.org does not support permanent deletion of packages" (yalniz unlist)

## TIER kararlari (kullanici istegi: "daha iyisi varsa kanitla")

### Seviye: TIER 3 (Kurumsal) — onceki TIER 2 onerisi geri cekildi
Onceki oneri "tek gelistirici" yanlis varsayimina dayaniyordu. /tier-select cevaplari (kullanici beyanindan):
| Soru | Cevap | Dayanak |
|---|---|---|
| S1 kim kullanacak | (d) genis kitle | "herkese fayda" + public NuGet |
| S2 veri kritikligi | (c) kritik | kutuphane veri tutmaz ama tuketicinin kimlik dogrulama (Jwt, Totp, Argon2), kalicilik (EF), sir (Key Vault) yolunda |
| S3 kullanici sayisi | (d) 1000+ | hedef: "en cok tercih edilen" |
| S4 sure | (d) 3 ay+ | surdurulebilir; v1.0.0 → v3.4.0 |
| S5 guvenlik | (d) 2FA + audit | kutuphane 2FA (Totp), JWT, yetkilendirme sunuyor |
Cogunluk (d) → TIER 3. TIER 4 kosulu (S2=d) yok. Ek: ekip olcegi (paralel ajanlar) Kurumsal tanimina uyar.
Duzeltme: raporda "TIER 3'te her K2 ise adversarial-review eklenir" dedim — yanlis; matris satiri yalniz
auth / odeme / veri silme / guvenlik kapisi degisikliklerini kapsar.

### Konum: mevcut tasarim (kok CLAUDE.md, commit'li) korunur — benim alternatifim elendi
On-beyan kriterleri: K1 tum oturumlar gorur (ana checkout, uygulama worktree'leri, paralel ajan worktree'leri,
bulut oturumlari) · K2 surum gecmisi + PR incelemesi (TIER degisimi ADR'lik karar) · K3 ekosistem degisikligi
gerektirmez · K4 public repo hijyeni (sir/kisisel veri yok, katkiciya anlamli).
| Secenek | K1 | K2 | K3 | K4 | Sonuc |
|---|---|---|---|---|---|
| A kok `CLAUDE.md` (mevcut v2) | var | var | var | var — CONTRIBUTING.md:77 zaten `CLAUDE.md`'ye yonlendiriyor, dosya hic olmamis (bos referans) | **Secildi** |
| B `.claude/CLAUDE.md` (benim onerim) | yok — gitignore'da, worktree/bulutta yok | yok | hook degisikligi | var | Elendi |
| C `CLAUDE.local.md` | yok — dokuman: yalniz olusturuldugu worktree'de | yok | hook + .gitignore | var | Elendi |
Rapor bulgusu 1a "bulgu degil" olarak kapatilir; ekosistemde degisiklik yok.

## Alternatifler (G1 ve G2 icin)
| Yaklasim | Arti | Eksi | Karar |
|---|---|---|---|
| G1-a global `ask` kurallari | dokumanla dogrulanmis oncelik; tum projeler; kod yok | her push'ta 1 onay penceresi | **Secildi** (Kabul Kapisi global kural) |
| G1-b proje allow satirlarini silmek | basit | auto modda siniflandirici yine sorgusuz calistirabilir | Elendi |
| G1-c branch-guard'a tag/push engeli | ince ayar | regex ile kabuk ayristirma (pilot bulgusu 4: 7 acik) | Elendi |
| G2-a korumali environment + zorunlu onaylayici + environment secret | insan tiklamasi Claude araclarinin disinda; anahtar onaysiz job'a verilmez | GitHub UI adimi (kullanici) | **Secildi** |
| G2-b yalniz workflow_dispatch | tag kazasi biter | `gh workflow run` ile yine sorgusuz tetiklenebilir | Elendi |
| G2-c NuGet trusted publishing (OIDC) | uzun omurlu anahtar yok | kapi degil (onay eklemez); ayri currency-check | Ayri is |

## Gorevler (risk sirasi; G0 on kosul — proje tarafindaki kapilari belirler)

### G0: Proje profili — kok CLAUDE.md (proje, Sari)
- Teslim: `CLAUDE.md` (Ingilizce, public): `## TIER: 3` + secilen icerik (kullanici secer: yalniz TIER ya da
  TIER + takim konvansiyonlari — her madde repo ile dogrulanir) + `decisions/2026-10-14-claude-process-profile.md`
- Ornek: yeni oturum → "Proje profili: Kurumsal (TIER 3) — kaynak: proje CLAUDE.md ... 'Kurumsal' sutunu"
- Test: hook'u proje dizininde calistir → beklenen cumle; docfx kapsami disinda (yalniz docs/**, README, CHANGELOG, CONTRIBUTING)
- Kayit: feature branch → kabul → commit + push + PR

### G1: Kabul Kapisi zorlamasi — global ask (.claude, Kirmizi: izin modeli)
- Teslim: `settings.json` `permissions.ask` += Bash ve PowerShell icin: `git push *`, `git -C * push *`,
  `gh pr create *`, `gh pr merge *`, `gh release create *`, `gh workflow run *`
- Ornekler: projede `git push origin feature/x` → onay penceresi · `git push --force` → hala deny ·
  `.claude`'da /finish → sorusuz (push script icinde) · proje `settings.local.json` allow'u ezilir (deny→ask→allow)
- Testler (once yazilir): ekosistem "ask git push (Bash+PowerShell)", "ask gh workflow run", "deny force push korunur",
  "settings.json gecerli JSON"; canli kanarya: sonraki oturumda `git push --dry-run` → pencere
- Kayit: ozet + onay → /finish

### G2: NuGet yayin kapisi (proje + GitHub, Kirmizi: 3. parti + guvenlik modeli)
- Sira zorunlu: 1) **Kullanici (GitHub UI):** environment `nuget` · required reviewer = omerkck41 · prevent
  self-review KAPALI (Claude senin kimliginle push eder; acik olursa onaylayamazsin) · deployment kurali: tag `v*.*.*`
  + branch `main` (workflow_dispatch main ref'inde kosar; yalniz tag kurali dispatch yolunu reddeder — kor nokta B6) ·
  `NUGET_API_KEY` environment secret · ayrica main branch korumasi (Kurumsal "PR + branch koruma", "CI her zaman"):
  PR zorunlu + build-test zorunlu durum kontrolu — sonradan dogrulandi: zaten var (ilk deneme guard'in EXFIL
  yanlis alarmina takilmisti, `gh api` engelli degil) · 2) ben: `nuget-publish.yml` publish job'una `environment: nuget` ·
  3) merge sonrasi kullanici repo duzeyi `NUGET_API_KEY`'i siler (yoksa ortam disi job anahtari okuyabilir)
- Ornekler: `v3.5.0` tag → Actions "Waiting for review"; onaysiz → yayin yok, anahtar verilmez; onay → yayin
- Kanarya (kullanici tetikler): workflow_dispatch tag=v3.4.0 (mevcut; `--skip-duplicate`) → onay bekler → reddet
- Kurumsal kapilar: ADR `docs/adr/0027-nuget-publish-approval-gate.md` · /code-review · /adversarial-review
- Kayit: feature branch → kabul → commit + push + PR

### G3: CPM kor noktasi — currency-guard + currency-check (.claude, Sari)
- Teslim: currency-guard dosya deseni += `Directory\.Packages\.props|Directory\.Build\.props`;
  currency-check `paths` += `**/Directory.Packages.props`, `**/Directory.Build.props`
- Ornekler: props'a `<PackageVersion Include="AutoMapper" Version="15.0.0" />` → ask + "Yerine: Mapperly" ·
  temiz props → sessiz
- Testler (once): hook suite 3 vaka (eskimis → ask, temiz → sessiz, Directory.Build.props → kapsanir)
- Bilinen etki: bu projede props tamamen yeniden yazilirsa MediatR satiri (ADR-0021 ile deprecate edilmis wrapper) ask uretir — beklenen
- Kayit: ozet + onay → /finish

### G4: Paket yayini kurali (.claude, davranis degisimi)
- Teslim: `rules/release.md` "Paket Yayini (NuGet/npm/PyPI)" bolumu: tag push = canli; geri alinamaz (NuGet yalniz
  unlist); korumali environment + zorunlu onaylayici + environment secret; SemVer kirici degisiklik + CHANGELOG
  go/no-go; tag yalniz kullanici kabuluyle. `/release` SKIP satiri bu bolume yonlendirir.
- Testler (once): ekosistem "release.md Paket Yayini bolumu", "release SKILL SKIP → release.md"
- Kayit: ozet + onay → /finish

### G5: session-context .NET sinyali + TIER satiri toleransi (.claude, Sari)
- Teslim: MANIFESTS += `Directory.Packages.props`, `src/*/*.csproj`, `src/*/*/*.csproj`; XML manifestte odeme
  yalniz `Include="..."` paket adlarinda aranir; TIER satiri sonunda `(...)` / `# ...` / `— ...` aciklamasina izin
- Ornekler: props'ta Stripe.net → "odeme bagimliligi" · csproj aciklamasinda "64-stripe locking" → odeme yok ·
  `TIER: 2 (Standart)` → Standart · `TIER: 2-3` → tanimsiz
- Testler (once): hook suite 5 vaka (yukaridaki 4 + mevcut 8 vaka degismeden gecer)
- Kayit: ozet + onay → /finish

### Degisiklik yapilmayacaklar (gerekceli)
- 3b /finish korumasi: `finish.md` goreli `bash scripts/finish.sh` kullaniyor + script sabit `$HOME/.claude` →
  proje repo'suna dokunamaz; ek kod gereksiz.
- 1a hook konum destegi: yukaridaki karsilastirma — mevcut tasarim daha iyi.

## Etki haritasi
- Degisen (.claude): settings.json (G1), hooks/currency-guard.sh (G3), skills/currency-check/SKILL.md (G3),
  rules/release.md + skills/release/SKILL.md (G4), hooks/session-context.py (G5), tests/ (her biri)
- Degisen (proje): CLAUDE.md (yeni), decisions/2026-10-14-claude-process-profile.md, .github/workflows/nuget-publish.yml,
  docs/adr/0027-*.md
- Referans veren: role.md §2.1 (Kabul), workflow.md Kapi Matrisi, tier-select.md, CONTRIBUTING.md:77, docfx.json
- Tutarli kalmali: deny > ask (force push), /finish akisi, cekirdek yuk butcesi (CLAUDE.md < 200), hook gecikmesi
- Koruyan testler: hook suite (171) + ekosistem (197) + yeni vakalar
- Etkilenenler: diger tum projeler (G1: her push'ta onay penceresi; G3, G5: .NET projeleri)

## Inceleme odagi (on-mortem cikti — her biri bir testte)
1. G1 kurali /finish'i kirar → test: finish --dry-run sorusuz
2. G2 sira hatasi: workflow environment'tan once merge → korumasiz ortam/anahtar → sira GitHub adimi once
3. G2 self-review acik → yayin kilitlenir → plan ayari KAPALI
4. G3 her props yazmasinda yanlis alarm → temiz props testi
5. G5 TIER toleransi belirsiz satiri kabul eder (`TIER: 2-3`) → negatif test

## Kor noktalar (/blindspots — plana islenenler)
- B1 Ekip olcegi: G1 kullanici kapsamli (`~/.claude`) — bulut oturumlari/diger makineler gormez. Sunucu tarafi
  kapilar (G2 environment + main branch korumasi) tum oturumlari kapsar; proje `.claude/` gitignore'da oldugu icin
  paylasilan proje izni ayri karar (oneri: simdilik hayir).
- B4 Arka plan ajanlari ask'e takilirsa push olmaz (guvenli yonde basarisiz); push'u koordinator kabulden sonra yapar.
- B6 workflow_dispatch ref = main → environment kurali main'e de izin verir (G2'ye islendi).
- B8 Onay yorgunlugu: G1 her push'ta (CI duzeltme push'u dahil) pencere acar — kapsam kullanici karari
  (her push / yalniz geri alinamazlar: merge, tag, release, dispatch, PR acma).

## Varsayimlar
| Varsayim | Dogrulama | Kritiklik |
|---|---|---|
| Repo public (Free'de required reviewers yalniz public) | G2 adim 1'de GitHub UI | Yuksek |
| Push kullanici kimligiyle (Actions'ta tetikleyen = omerkck41) | G2 kanaryasinda gorulur | Orta |
| settings.json degisikligi yeni oturumda kesin etkin | G1 canli kanarya | Dusuk |

## Basari olcutleri ve geri alma
- Olcut: hook + ekosistem testleri yesil (yeni vakalar dahil) · G1 kanarya penceresi · G2 kanarya "Waiting for review" ·
  yeni oturumda "Kurumsal (TIER 3)" satiri
- Geri alma: `.claude` her gorev ayri commit → `git revert` (`v1-baseline` etiketi duruyor) · proje PR'lari revert ·
  GitHub environment silinir / repo secret yeniden eklenir

## Yan bulgular (bu planin disinda)
- context7 plugin surumu degisti (8d392c8 → 8bd00f0) → tedarik zinciri incelemesi (ayri gorev)
- Ekip olcegi: proje konvansiyonlari yalniz yerel hafizada (AwesomeAssertions, xunit v2 kilidi, CPM restore) —
  bulut oturumlari ve diger makineler goremez → G0 icerik seceneginde
