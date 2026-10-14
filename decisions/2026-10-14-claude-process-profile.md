# Karar: Claude Code süreç profili TIER 3 (Kurumsal) — kök CLAUDE.md

- **Tarih:** 2026-10-14
- **Durum:** Kabul edildi (2026-10-14, bakımcı)
- **Bağlam:** Ekosistem v2 (`~/.claude`) proje profilini kök `CLAUDE.md`'deki `TIER:` satırından okur
  (`hooks/session-context.py`); satır yoksa profil "tanımsız" kalır ve Kapı Matrisi'nin hangi sütunu
  uygulanacağı belirsizdir. Bu projede satır yoktu (oturum başı: "tanımsız · sinyal: CI").

## Seviye: TIER 3
`/tier-select` cevapları (bakımcı beyanı, 2026-10-14: "herkese fayda sağlayacak, sürdürülebilir kütüphane;
Claude ekip olarak geliştiriyor, ben denetleyici ve karar vericiyim"):

| Soru | Cevap | Dayanak |
|---|---|---|
| Kim kullanacak | (d) geniş kitle | public NuGet paketleri |
| Veri kritikliği | (c) kritik | tüketicinin kimlik doğrulama (Jwt, Totp, Argon2), kalıcılık (EF), sır (Key Vault) yolunda |
| Kullanıcı sayısı | (d) 1000+ | hedef: en çok tercih edilen |
| Süre | (d) 3 ay+ | v1.0.0 → v3.4.0 |
| Güvenlik | (d) 2FA düzeyi | kütüphane 2FA (Totp), JWT, yetkilendirme sunuyor |

Çoğunluk (d) → TIER 3. TIER 4 koşulu (veri: finansal/yasal) yok. Çok-ajanlı geliştirme "ekip" tanımına uyar.
Elenen: TIER 2 — "tek geliştirici" varsayımına dayanıyordu; bakımcı düzeltti.

## Konum: kök `CLAUDE.md` (commit'li)
Kriterler (değerlendirmeden önce): tüm oturumlar görür · sürüm geçmişi + PR · ekosistem değişikliği gerektirmez · public repo hijyeni.

| Seçenek | Sonuç |
|---|---|
| Kök `CLAUDE.md` | Seçildi — git ile her worktree/klon/bulut oturumunda var; `CONTRIBUTING.md` zaten bu dosyaya yönlendiriyordu (dosya hiç yoktu) |
| `.claude/CLAUDE.md` | Elendi — bu repoda `.claude/` gitignore'da; worktree/bulut oturumları görmez; hook değişikliği gerekir |
| `CLAUDE.local.md` | Elendi — doküman: "a gitignored CLAUDE.local.md only exists in the worktree where you created it" |

## Kaynaklar
- https://code.claude.com/docs/en/memory (erişim 2026-10-14) — proje talimatı konumları, worktree notu
- `~/.claude/rules/workflow.md` Kapı Matrisi · `~/.claude/commands/tier-select.md`
