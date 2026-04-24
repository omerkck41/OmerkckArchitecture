# Logging

Serilog tabanli yapilandirilmis loglama. Trace correlation, JSON output,
rolling file destegi.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Logging.Serilog` | `AddKckSerilog()` + `KckSerilogBuilder` |

## Setup

```csharp
builder.Services.AddKckSerilog(log => log
    .WithApplicationName("OrderService")
    .WriteToCompactJson()
    .WriteToFile("logs/app-.log", RollingInterval.Day)
    .WithTraceCorrelation());
```

## Builder API

| Metod | Aciklama |
|---|---|
| `WithApplicationName(name)` | `Application` property'si her loga eklenir |
| `WriteToConsole()` | Standard console sink (rendered text) |
| `WriteToCompactJson()` | `RenderedCompactJsonFormatter` — log aggregator uyumlu |
| `WriteToFile(path, interval)` | Rolling file sink (varsayilan: Day, 31 dosya) |
| `WithTraceCorrelation(enabled)` | `TraceId`, `SpanId`, `ParentSpanId` property'leri |
| `Configure(action)` | Raw `LoggerConfiguration` access |

## Trace Correlation

`WithTraceCorrelation(true)` varsayilandir. `Activity.Current` varsa her log
event'ine `TraceId` ve `SpanId` eklenir — OpenTelemetry ile log/trace
iliskilendirme saglar.

## Log Levels

Varsayilan minimum `Information`. Development'ta artirmak icin:

```csharp
builder.Services.AddKckSerilog(log => log
    .Configure(c => c.MinimumLevel.Debug()));
```

## Yapilandirilmis Log Ornekleri

Dogru kullanim (template):

```csharp
logger.LogInformation("User {UserId} placed order {OrderId} for {Amount}",
    userId, orderId, amount);
```

Yasak (string interpolation):

```csharp
// YANLIS — logger parametre extractionu yapmaz
logger.LogInformation($"User {userId} placed order {orderId}");
```

## PII / Sensitive Data

- Sifre, token, CVV asla logla — middleware'de filter ekle
- `ILoggerFactory`'den olusturulan logger'da `ForContext<T>()` scope pattern
  kullanilabilir
- GDPR: PII'yi hash veya masked log (E.164 → masked son 4 hane)

## Log Retention

`WriteToFile`'da default `retainedFileCountLimit: 31` (31 gun). Artirmak icin
`Configure()` ile `.WriteTo.File(..., retainedFileCountLimit: 90)` ekle.
