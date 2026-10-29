# Karar: actionlint + zizmor CI'da (workflow-lint.yml)

- Tarih: 2026-10-30
- Bağlam: rules/release.md "Workflow değiştiğinde: actionlint + zizmor" diyor; ikisi yerelde kurulu değildi ve hiçbir
  CI işi çalıştırmıyordu (todo, 2026-10-26). Kullanıcı kararı: yerel tek seferlik koşu yerine CI'a ekle.
- Pre-commitment: ölçütler — resmi kaynak, sürüm + bütünlük sabitlenmiş, yeni üçüncü taraf action yok, secret
  gerekmez, izin verici lisans, bakımı süren proje. Kaynak: PyPI JSON API, GitHub Releases/License API.
- Adaylar:
  - A — zizmor: hash'li `requirements.txt` + venv'de `pip --require-hashes --only-binary`; actionlint: resmi sürüm
    arşivi + yayımlanan SHA-256 ile `sha256sum -c` (ilk denemedeki `go install …@v1.7.12` Sonar githubactions:S8545
    "öngörülemeyen sürüm" verdi). Yeni action yok. Seç.
  - B — `zizmorcore/zizmor-action` + `rhysd/actionlint` Docker imajı: iki yeni üçüncü taraf bağımlılık (SHA/digest
    sabitlemesi ve Dependabot takibi gerekir). Ele.
  - C — yerelde tek seferlik koşu: kalıcı koruma sağlamaz. Ele (kullanıcı da seçmedi).
- Sürümler (erişim 2026-10-30):
  - zizmor 1.31.1 — PyPI, yayın 2026-10-15, MIT, bağımlılık yok; wheel `manylinux_2_28_x86_64` sha256
    58e878eb2dbd2c6183c105e1ac28c14f7dd1eca44a783f1fe2a69647d012c410 (https://pypi.org/pypi/zizmor/json).
  - actionlint v1.7.12 — en son sürüm, yayın 2026-03-30, MIT; `actionlint_1.7.12_linux_amd64.tar.gz` sha256
    8aca8db96f1b94770f1b0d72b6dddcb1ebb8123cb3712530b08cc387b349a3d8 (https://github.com/rhysd/actionlint/releases).
  - `rules/deprecated-registry.md`: kayıt yok. EOL: CLI araçları, endoflife.date'te kayıt yok.
- Çalışma: `workflow-lint.yml`, yalnız `.github/workflows/**` ve gereksinim dosyası değişince; `permissions: contents:
  read`; zizmor `--offline` (token yok, ağ denetimleri yok).
- Güncelleme: zizmor → Dependabot `pip` (`/.github/workflow-lint`); actionlint → elle (sürüm + SHA-256 workflow'da).
