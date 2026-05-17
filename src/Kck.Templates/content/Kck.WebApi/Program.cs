using KckWebApiTemplate.Features.WeatherForecast;

var builder = WebApplication.CreateBuilder(args);

// ── Kck WebAPI Defaults ──────────────────────────────────────────────────────
// Registers: Serilog logging, in-memory caching, JWT (if configured), Argon2,
// in-memory event bus, exception handling, OpenTelemetry, rate limiting,
// security headers, and CORS.
builder.Services.AddKckWebApiDefaults(builder.Configuration);

// ── API Documentation ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// ── Your Application Services ────────────────────────────────────────────────
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();

// ────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────────────────────────
app.UseKckWebApiDefaults();  // exception handling + rate limiting + security headers + CORS

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
