using Kck.Exceptions.AspNetCore.Extensions;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Sample.WebApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ══════════ LOGGING ══════════
builder.Services.AddKckSerilog(serilog => serilog
    .WriteToConsole()
    .WithApplicationName("Kck.Sample.WebApi")
    .WithTraceCorrelation());

// ══════════ PERSISTENCE ══════════
builder.Services.AddKckPersistence(persistence => persistence
    .UseEntityFramework<AppDbContext>(db => db.UseInMemoryDatabase("SampleWebApi")));

// ══════════ CACHING ══════════
builder.Services.AddKckCachingInMemory();

// ══════════ OBSERVABILITY ══════════
builder.Services.AddKckObservability(obs =>
{
    obs.UseOpenTelemetry(otel =>
    {
        otel.ServiceName = "Kck.Sample.WebApi";
        otel.EnableTracing();
        otel.EnableMetrics();
    });
    obs.UseHealthChecks(_ => { });
});

// ══════════ EVENT BUS ══════════
builder.Services.AddKckEventBus(eventBus =>
{
    if (builder.Environment.IsDevelopment())
    {
        eventBus.UseInMemory();
    }
    else
    {
        eventBus.UseRabbitMq(rabbit =>
        {
            rabbit.HostName = builder.Configuration["RabbitMq:Host"]
                ?? throw new InvalidOperationException("Configuration 'RabbitMq:Host' is required.");
            rabbit.UserName = builder.Configuration["RabbitMq:Username"]
                ?? throw new InvalidOperationException("Configuration 'RabbitMq:Username' is required.");
            rabbit.Password = builder.Configuration["RabbitMq:Password"]
                ?? throw new InvalidOperationException("Configuration 'RabbitMq:Password' is required.");
        });
    }
    eventBus.RegisterHandlersFromAssembly(typeof(Program).Assembly);
});

// ══════════ EXCEPTION HANDLING ══════════
builder.Services.AddKckExceptionHandling();

// ══════════ ASP.NET CORE ══════════
builder.Services.AddKckAspNetCore(aspnet =>
{
    aspnet.UseSecurityHeaders();
    aspnet.UseRateLimiting(rate =>
    {
        rate.PermitLimit = 100;
        rate.WindowInSeconds = 60;
    });
    aspnet.UseCorsPolicy(origins: ["http://localhost:3000"]);
    aspnet.AddInputSanitizer();
    aspnet.AddCookieManager();
    aspnet.AddSessionManager();
});

builder.Services.AddControllers();

var app = builder.Build();

// ══════════ MIDDLEWARE PIPELINE ══════════
app.UseKckExceptionHandling();
app.UseKckAspNetCore();
app.MapKckHealthChecks("/health");
app.MapControllers();

app.Run();
