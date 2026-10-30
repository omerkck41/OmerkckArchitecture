using Kck.Persistence.EntityFramework.Extensions;
using KckWebApiTemplate.Api.Data;
//#if (IncludeSample)
using KckWebApiTemplate.Api.Orders;
//#endif
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog, ProblemDetails for exceptions, rate limiting, security headers, CORS (Kck:AspNetCore:CorsOrigins) and the
// health check service.
builder.Services.AddKckWebApiDefaults(builder.Configuration);

// Persistence: repositories and IUnitOfWork over AppDbContext. AddAuditInterceptor fills CreatedDate / ModifiedDate and
// turns deletes of soft-deletable entities into soft deletes; AddDomainEventHandlers dispatches domain events to the
// IDomainEventHandler<T> types in this assembly after the data is saved (after the commit inside a transaction).
//#if (UseSqlite)
// A local file database in development only; any other environment must configure the connection string.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? (builder.Environment.IsDevelopment()
        ? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "KckWebApiTemplate.db")}"
        : throw new InvalidOperationException("Connection string 'Default' is not configured (see README.md)."));
//#else
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is not configured. Set it with `dotnet user-secrets set ConnectionStrings:Default \"...\"` (see README.md).");
//#endif
builder.Services.AddKckPersistence(persistence => persistence
//#if (UseSqlite)
    .UseEntityFramework<AppDbContext>(options => options.UseSqlite(connectionString))
//#elif (UsePostgres)
    .UseEntityFramework<AppDbContext>(options => options.UseNpgsql(connectionString))
//#else
    .UseEntityFramework<AppDbContext>(options => options.UseSqlServer(connectionString))
//#endif
    .AddAuditInterceptor()
    .AddDomainEventHandlers(typeof(Program).Assembly));
builder.Services.AddHealthChecks().AddKckEfCoreCheck<AppDbContext>();

// JWT bearer authentication. In development, create a token with `dotnet user-jwts create` (see README.md).
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

// /health, /health/live, /health/ready, /health/startup: no authentication, and ahead of the rate limiter so probes
// are never throttled. Responses list check names and durations; restrict them to your network in production.
app.MapKckHealthChecks();

app.UseKckWebApiDefaults();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    // OpenAPI document at /openapi/v1.json and an interactive reference at /scalar (development only).
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.DisableDefaultFonts());

    // Creates the schema on first run. Switch to EF Core migrations before going to production (README.md).
    await app.Services.EnsureDatabaseCreatedAsync();
}

//#if (IncludeSample)
app.MapOrderEndpoints();
//#endif

await app.RunAsync();

/// <summary>Entry point; public so the test project can start the application.</summary>
public partial class Program;
