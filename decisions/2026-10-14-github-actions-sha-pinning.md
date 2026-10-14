# Karar: GitHub Actions'i tam commit SHA'ya sabitle

- **Tarih:** 2026-10-14
- **Bağlam:** Tüm workflow'lar action'ları taşınabilir etiketle (`actions/checkout@v6`) kullanıyordu;
  `benchmark-action/github-action-benchmark@v1` ise etiket değil **branch** (`v1`) referansıydı. Etiket/branch
  upstream'de yeniden yönlendirilebilir; ele geçirilen bir action, NuGet yayın job'u dahil tüm CI'da kod
  çalıştırabilir (OWASP CI/CD Top 10 — CICD-SEC-3, Dependency Chain Abuse).
- **Karar:** 11 workflow'daki 32 `uses:` satırı `owner/repo@<40 karakter SHA> # vX.Y.Z` biçimine sabitlendi.
  SHA, mevcut etiketin **bugün** gösterdiği commit'tir → çalışan kod değişmez. Güncelleme: `.github/dependabot.yml`
  içindeki mevcut `github-actions` girdisi (haftalık).
- **Yorum biçimi:** Major etiket (`# v6`) yerine aynı commit'i gösteren tam sürüm etiketi (`# v6.1.0`).
  - Elenen `# v6`: upstream `v6`'yı ilerlettiği an yorum pinli SHA ile çelişir (zizmor `ref-version-mismatch`).
  - Elenen "en güncel sürüme pinle": davranış değişikliği; major geçişler açık Dependabot PR'larında (#119, #127).
- **Sürümler (çözümleme 2026-10-14, `gh api repos/<r>/git/ref/tags/<t>`, annotated etiketler deref edildi):**

  | Action | Eski ref | SHA | Yorum |
  |---|---|---|---|
  | actions/checkout | v6 | d23441a48e516b6c34aea4fa41551a30e30af803 | v6.1.0 |
  | actions/setup-dotnet | v5 | 26b0ec14cb23fa6904739307f278c14f94c95bf1 | v5.4.0 |
  | actions/upload-artifact | v7 | cf430e030ddbb5b0abf93d22962f4752f3646cd9 | v7.0.2 |
  | actions/upload-pages-artifact | v5 | fc324d3547104276b827a68afc52ff2a11cc49c9 | v5.0.0 |
  | actions/deploy-pages | v5 | 368f82528645a54fb793d4d04e342629a3f51346 | v5.0.1 |
  | actions/setup-java | v5 | b6effb05e454b25005698d916606bdc6ffcbf961 | v5.7.0 |
  | benchmark-action/github-action-benchmark | v1 (branch) | 4322e5726e6334590d251fc4f92bec0efafc45dc | v1.22.2 |
  | github/codeql-action (init, analyze) | v4 (annotated) | 24c54180a607b1449ed407dd24f251e4e9147c8d | v4.38.3 |
  | dependabot/fetch-metadata | v3 | 25dd0e34f4fe68f24cc83900b1fe3fe149efef98 | v3.1.0 |
  | release-drafter/release-drafter | v7 | 978bb0b30c5ee8cdd332ced253758157150d43f0 | v7.10.0 |

- **Doğrulama:** Her çift için `compare/<tag>...<sha>` = `identical` ve etiket deref = SHA (10/10). SHA'lar
  upstream deponun kendi etiketlerinden alındı (fork commit'i / impostor commit riski yok). YAML ayrıştırma 11/11 OK.
- **Kaynaklar:**
  - https://github.blog/changelog/2022-10-31-dependabot-now-updates-comments-in-github-actions-workflows-referencing-action-versions/ (erişim: 2026-10-14) — Dependabot SHA yanındaki semver yorumunu günceller
  - https://docs.zizmor.sh/audits/ (erişim: 2026-10-14) — `unpinned-uses` (v1.20.0+ varsayılan: tüm action'lar hash-pin), `ref-version-mismatch` (v1.14.0+)
  - https://owasp.org/www-project-top-10-ci-cd-security-risks/ — CICD-SEC-3
- **EOL:** Tüm pinli action'lar `node24` veya `composite` çalıştırıcı kullanıyor (Node 20 göçünden etkilenmez).
- **Not:** Pinleme, upstream'in yeni (kötü niyetli olabilecek) sürümünü engellemez; yalnız sessiz değişimi
  engeller. Dependabot PR'larında SHA değişikliği yine incelenmeli.
