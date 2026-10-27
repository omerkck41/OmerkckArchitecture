//#if (IncludeSample)
using KckWebApiTemplate.Features.WeatherForecast;

//#endif
var builder = WebApplication.CreateBuilder(args);

// ── Kck WebAPI Defaults ──────────────────────────────────────────────────────
// Registers: Serilog logging, in-memory caching, JWT (if configured), Argon2,
// in-memory event bus, exception handling, OpenTelemetry, rate limiting,
// security headers, and CORS.
builder.Services.AddKckWebApiDefaults(builder.Configuration);

// ── API Documentation ────────────────────────────────────────────────────────
// OpenAPI document at /openapi/v1.json (Development). Add Swagger UI or Scalar if you want an interactive UI.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// ── Your Application Services ────────────────────────────────────────────────
//#if (IncludeSample)
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
//#endif

// ────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────────────────────────
app.UseKckWebApiDefaults();  // exception handling + rate limiting + security headers + CORS

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
