using Kck.Persistence.Abstractions.Security;
using Kck.Persistence.EntityFramework.Extensions;
using Kck.Sample.Orders.Data;
using Kck.Sample.Orders.Domain;
using Kck.Sample.Orders.Features.Customers;
using Kck.Sample.Orders.Features.Orders;
using Kck.Sample.Orders.Features.Products;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog, ProblemDetails for exceptions, rate limiting, security headers, the in-memory cache and event bus, and the
// health check service.
builder.Services.AddKckWebApiDefaults(builder.Configuration);

// Persistence: repositories and IUnitOfWork over OrdersDbContext. AddAuditInterceptor fills CreatedDate / ModifiedDate
// and turns deletes of soft-deletable entities into soft deletes that follow the model's delete rules;
// AddDomainEventHandlers dispatches domain events to the IDomainEventHandler<T> types in this assembly after the commit.
builder.Services.AddKckPersistence(persistence => persistence
    .UseEntityFramework<OrdersDbContext>(options => options.UseOrdersDatabase(builder.Configuration, builder.Environment))
    .AddAuditInterceptor()
    .AddDomainEventHandlers(typeof(Program).Assembly));
builder.Services.AddSingleton<IFilterPropertyWhitelist<Order>, OrderFilterWhitelist>();
builder.Services.AddHealthChecks().AddKckEfCoreCheck<OrdersDbContext>();

// Integration events: OrderPlacedHandler publishes OrderConfirmationRequested; the bundle registered the in-memory bus,
// this registers the IEventHandler<T> types of this assembly.
builder.Services.AddKckEventBus(bus => bus.UseInMemory().RegisterHandlersFromAssembly(typeof(Program).Assembly));

// OpenTelemetry traces and metrics for requests, EF Core commands and HttpClient calls. Nothing is exported until the
// standard OTEL_EXPORTER_OTLP_ENDPOINT variable is set (http://localhost:4317 for a local collector; Aspire sets it).
builder.Services.AddKckObservability(observability => observability.UseOpenTelemetry(otel =>
{
    otel.ServiceName = "kck-sample-orders";
    otel.EnableTracing().EnableMetrics();
    if (builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] is { Length: > 0 } endpoint)
        otel.UseOtlpExporter(endpoint);
}));

// JWT bearer authentication. In development, create a token with `dotnet user-jwts create` (README.md).
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

// /health, /health/live, /health/ready, /health/startup: no authentication, and ahead of the rate limiter.
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

    // Creates the schema on first run. Use EF Core migrations before going to production.
    await app.Services.EnsureDatabaseCreatedAsync();
}

app.MapCustomerEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();

await app.RunAsync();

/// <summary>Entry point; public so the test project can start the application.</summary>
public partial class Program;
