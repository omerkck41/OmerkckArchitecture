# Kck Framework — Konfigurasyon Rehberi

IDE autocomplete için projenizin `appsettings.json` dosyasına şu satırı ekleyin:

```json
{
  "$schema": "https://raw.githubusercontent.com/omerkck41/OmerkckArchitecture/main/schemas/appsettings.kck.schema.json"
}
```

Tüm ayarlar `"Kck"` ana anahtarı altında toplanır. Kullanmadığınız provider'ların bölümlerini eklemenize gerek yoktur.

---

## Caching

### Redis (`Kck.Caching.Redis`)

```json
"Kck": {
  "Caching": {
    "Redis": {
      "Configuration": "localhost:6379",
      "InstanceName": "myapp:",
      "DefaultExpirationSeconds": 3600
    }
  }
}
```

| Alan | Tür | Zorunlu | Varsayılan | Açıklama |
|---|---|---|---|---|
| `Configuration` | string | Evet | — | StackExchange.Redis bağlantı dizisi |
| `InstanceName` | string | Hayır | `""` | Tüm anahtarlara eklenen önek |
| `DefaultExpirationSeconds` | int | Hayır | `3600` | TTL (saniye) |

### InMemory (`Kck.Caching.InMemory`)

```json
"Kck": {
  "Caching": {
    "InMemory": {
      "SizeLimit": 10000,
      "DefaultExpirationSeconds": 300
    }
  }
}
```

---

## Security

### JWT (`Kck.Security.Jwt`)

```json
"Kck": {
  "Security": {
    "Jwt": {
      "Issuer": "https://auth.example.com",
      "Audience": "https://api.example.com",
      "Algorithm": "RS256",
      "AccessTokenExpirationMinutes": 60,
      "RefreshTokenExpirationDays": 30
    }
  }
}
```

| Alan | Tür | Zorunlu | Varsayılan | Açıklama |
|---|---|---|---|---|
| `Issuer` | string | Evet | — | Token yayıncısı URI |
| `Audience` | string | Evet | — | Beklenen token kitlesi |
| `Algorithm` | string | Hayır | `RS256` | `RS256` / `HS256` / ... |
| `AccessTokenExpirationMinutes` | int | Hayır | `60` | Erişim token ömrü |
| `RefreshTokenExpirationDays` | int | Hayır | `30` | Yenileme token ömrü |

### Argon2 (`Kck.Security.Argon2`)

```json
"Kck": {
  "Security": {
    "Argon2": {
      "DegreeOfParallelism": 4,
      "MemorySize": 65536,
      "Iterations": 4
    }
  }
}
```

---

## EventBus

### RabbitMQ (`Kck.EventBus.RabbitMq`)

```json
"Kck": {
  "EventBus": {
    "RabbitMq": {
      "HostName": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest",
      "VirtualHost": "/",
      "ExchangeName": "kck.events"
    }
  }
}
```

### Azure Service Bus (`Kck.EventBus.AzureServiceBus`)

```json
"Kck": {
  "EventBus": {
    "AzureServiceBus": {
      "ConnectionString": "Endpoint=sb://...",
      "TopicName": "kck-events"
    }
  }
}
```

---

## Messaging

### MailKit (`Kck.Messaging.MailKit`)

```json
"Kck": {
  "Messaging": {
    "MailKit": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UserName": "user@example.com",
      "Password": "app-password",
      "UseSsl": true,
      "FromAddress": "no-reply@example.com",
      "FromName": "My App"
    }
  }
}
```

### SendGrid (`Kck.Messaging.SendGrid`)

```json
"Kck": {
  "Messaging": {
    "SendGrid": {
      "ApiKey": "SG.xxx",
      "FromEmail": "no-reply@example.com",
      "FromName": "My App"
    }
  }
}
```

### Amazon SES (`Kck.Messaging.AmazonSes`)

```json
"Kck": {
  "Messaging": {
    "AmazonSes": {
      "AccessKeyId": "AKIA...",
      "SecretAccessKey": "...",
      "Region": "eu-west-1",
      "FromAddress": "no-reply@example.com"
    }
  }
}
```

---

## Background Jobs (Kck.Bundle.WorkerService)

```json
"Kck": {
  "BackgroundJobs": {
    "Provider": "Hangfire",
    "HangfireWorkerCount": 5
  }
}
```

> **Uyarı:** `Provider` değeri `"Hangfire"` veya `"Quartz"` olabilir. İkisini aynı anda kaydetmeyin.

---

## ASP.NET Core (Kck.Bundle.WebApi)

```json
"Kck": {
  "AspNetCore": {
    "RateLimitPermitLimit": 100,
    "RateLimitWindowSeconds": 60,
    "SecurityHeaders": true,
    "CorsOrigins": ["https://app.example.com"]
  }
}
```

---

## Tam Örnek (`appsettings.json`)

```json
{
  "$schema": "https://raw.githubusercontent.com/omerkck41/OmerkckArchitecture/main/schemas/appsettings.kck.schema.json",
  "Kck": {
    "Security": {
      "Jwt": {
        "Issuer": "https://auth.example.com",
        "Audience": "https://api.example.com"
      }
    },
    "Caching": {
      "Redis": {
        "Configuration": "localhost:6379",
        "InstanceName": "myapp:"
      }
    },
    "EventBus": {
      "RabbitMq": {
        "HostName": "localhost",
        "UserName": "guest",
        "Password": "guest"
      }
    }
  }
}
```
