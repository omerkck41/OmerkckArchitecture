using System.Security.Claims;
using System.Text.Encodings.Web;
//#if (IncludeSample)
using Kck.Core.Abstractions.Entities;
//#endif
using KckWebApiTemplate.Api.Data;
//#if (IncludeSample)
using KckWebApiTemplate.Api.Orders;
//#endif
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
//#if (UseSqlite)
using Microsoft.Data.Sqlite;
//#else
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
//#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KckWebApiTemplate.Api.Tests;

/// <summary>
/// Starts the API in memory against its own database, with a test authentication scheme instead of JWT.
/// Create one per test: every instance gets a fresh database.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
//#if (UseSqlite)
    // A named in-memory database lives while at least one connection to it is open.
    private readonly string _connectionString = $"Data Source=file:{Guid.NewGuid():N}?mode=memory&cache=shared";
    private readonly SqliteConnection _keepAlive;

    public ApiFactory()
    {
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }
//#else
    // The API's connection string 'Default' (user secrets or environment) with a database name of its own, created on
    // start-up and dropped on dispose. It is passed with UseSetting: Program.cs reads the connection string before the
    // host is built, where ConfigureAppConfiguration changes are not visible yet.
    private readonly string _databaseSuffix = $"_test_{Guid.NewGuid():N}";
    private readonly string _connectionString;

    public ApiFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Set ConnectionStrings:Default for the API to run these tests (README.md).");
        _connectionString = WithDatabaseSuffix(connectionString, _databaseSuffix);
    }

    private static string WithDatabaseSuffix(string connectionString, string suffix)
    {
        var connection = new DbConnectionStringBuilder { ConnectionString = connectionString };
        var key = new[] { "Database", "Initial Catalog" }.FirstOrDefault(connection.ContainsKey)
            ?? throw new InvalidOperationException("ConnectionStrings:Default must name a database (Database=...).");
        connection[key] = $"{connection[key]}{suffix}";
        return connection.ConnectionString;
    }
//#endif

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "test-user");
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // creates the schema on start-up
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("Kck:AspNetCore:RateLimitPermitLimit", "100000");
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, null);
//#if (IncludeSample)
            // Records OrderPlaced next to the application's own handler (OrdersEndpointTests).
            services.AddSingleton<RecordedEvents>();
            services.AddScoped<IDomainEventHandler<OrderPlaced>, RecordingOrderPlacedHandler>();
//#endif
        });
    }

    public override async ValueTask DisposeAsync()
    {
//#if (!UseSqlite)
        await using (var scope = Services.CreateAsyncScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
            // Never drop a database this factory did not create.
            if (database.GetDbConnection().Database.EndsWith(_databaseSuffix, StringComparison.Ordinal))
                await database.EnsureDeletedAsync();
        }
//#endif
        await base.DisposeAsync();
//#if (UseSqlite)
        await _keepAlive.DisposeAsync();
//#endif
    }
}

/// <summary>Authenticates requests that carry the <see cref="HeaderName"/> header; others get 401.</summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string HeaderName = "X-Test-User";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var user))
            return Task.FromResult(AuthenticateResult.NoResult());

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, user.ToString())], SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
