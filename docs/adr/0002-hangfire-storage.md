# ADR-0002: Hangfire Storage Sadelestirme (MySqlStorage Kaldirma)

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

`Kck.BackgroundJobs.Hangfire` provider'i `Directory.Packages.props` icinde
`Hangfire.MySqlStorage` 2.0.3 (2022 son commit) paketini tutuyordu. Ancak:

- Paket hicbir `.csproj`'da referans edilmiyor
- `ServiceCollectionExtensions.AddKckBackgroundJobsHangfire` switch'i yalniz
  `"inmemory"` case'ini isliyor; `MySql`, `SqlServer` case'leri yok
- `HangfireOptions.StorageType` XML doc'u `"MySql"`/`"SqlServer"` diyor ama
  uygulama yok — misleading dokuman
- .NET 10 uyumu belirsiz (2022'den beri guncelleme yok)

Audit raporu (2026-04-20) bu paketi **KIRMIZI** (terk edilmis) olarak isaretledi.

## Karar

`Hangfire.MySqlStorage` paket versiyon kaydi kaldirildi. Provider sadece
`Hangfire.InMemory` destekliyor. Kullanicilar gelismis backend istiyorsa
kendi `IServiceCollection.AddHangfire` cagrilarinda dogrudan kendi
tercih ettikleri storage paketini (Hangfire.SqlServer, Hangfire.Redis, vb.)
ekleyebilir.

## Alternatifler Degerlendirildi

- **Guncel tut:** Hangfire.MySqlStorage son surumu 2.0.3. Daha yeni yok, bakim yok.
- **Hangfire.SqlServer'a gec:** Resmi paket aktif bakimda, .NET 10 uyumlu.
  Ancak bu provider'in `StorageType` switch'i bu case'i da implement etmiyordu.
  Kullanici dogrudan Hangfire'in kendi DI API'siyle ekleyebilir — ekstra sarma
  katmani deger katmiyor.
- **Tamamen kaldir (secildi):** Kullanilmayan paket, yanilitici dokuman. En
  cerrahi yaklasim.

## Sonuclar

**Olumlu:**
- Dependency yuzeyi azaldi — 1 dead package daha az
- Dokuman-kod uyumsuzlugu giderildi
- NU1902 benzeri surpriz CVE riski dustu

**Olumsuz:**
- MySQL storage isteyen kullanicilar kendi `AddHangfire` konfiguruna eklemeli —
  bu provider sadece InMemory + DI temel yapisini sunar

## Migration Notu

Bu provider'i kullanan kullanicilar icin breaking change YOK; `Hangfire.MySqlStorage`
zaten bu provider uzerinden aktive edilemiyordu. Dogrudan referans ekleyenler
etkilenmez.
