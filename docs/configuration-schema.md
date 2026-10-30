# Kck Framework — Configuration Guide

For IDE autocomplete, add the following line to your project's `appsettings.json`:

```json
{
  "$schema": "https://raw.githubusercontent.com/omerkck41/OmerkckArchitecture/main/schemas/appsettings.kck.schema.json"
}
```

All settings are grouped under the top-level `"Kck"` key. You do not need to add sections for providers you do not use.

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

| Field | Type | Required | Default | Description |
|---|---|---|---|---|
| `Configuration` | string | Yes | — | StackExchange.Redis connection string |
| `InstanceName` | string | No | `""` | Prefix added to all keys |
| `DefaultExpirationSeconds` | int | No | `3600` | TTL (seconds) |

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

| Field | Type | Required | Default | Description |
|---|---|---|---|---|
| `Issuer` | string | Yes | — | Token issuer URI |
| `Audience` | string | Yes | — | Expected token audience |
| `Algorithm` | string | No | `RS256` | `RS256` / `HS256` / ... |
| `AccessTokenExpirationMinutes` | int | No | `60` | Access token lifetime |
| `RefreshTokenExpirationDays` | int | No | `30` | Refresh token lifetime |

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

> **Warning:** The `Provider` value can be `"Hangfire"` or `"Quartz"`. Do not register both at the same time.

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

## Full Example (`appsettings.json`)

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
