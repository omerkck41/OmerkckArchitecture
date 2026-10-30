using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Kck.Core.Abstractions.Entities;
using Kck.EventBus.Abstractions;
using Kck.Sample.Orders.Data;
using Kck.Sample.Orders.Domain;
using Kck.Sample.Orders.Features.Orders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Sample.Orders.Tests;

/// <summary>
/// Starts the sample in memory with a test authentication scheme instead of JWT. Without a connection string every
/// instance gets its own SQLite in-memory database; with one (PostgreSQL) the database is created and dropped by it.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _provider;
    private readonly string _connectionString;
    private readonly SqliteConnection? _keepAlive;
    private readonly Action<IServiceCollection>? _configureServices;

    public ApiFactory(string? postgresConnectionString = null, Action<IServiceCollection>? configureServices = null)
    {
        _configureServices = configureServices;
        if (postgresConnectionString is null)
        {
            // A named in-memory database lives while at least one connection to it is open.
            _provider = "Sqlite";
            _connectionString = $"Data Source=file:{Guid.NewGuid():N}?mode=memory&cache=shared";
            _keepAlive = new SqliteConnection(_connectionString);
            _keepAlive.Open();
        }
        else
        {
            _provider = "Postgres";
            _connectionString = postgresConnectionString;
        }
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.HeaderName, "test-user");
        return client;
    }

    public async Task<T> QueryAsync<T>(Func<OrdersDbContext, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<OrdersDbContext>());
    }

    public Task<int> StockAsync(string sku) =>
        QueryAsync(db => db.Products.Where(p => p.Id == sku).Select(p => p.Stock).SingleAsync());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // creates the schema on start-up
        builder.UseSetting("Database:Provider", _provider);
        builder.UseSetting("ConnectionStrings:Orders", _connectionString);
        builder.UseSetting("Kck:AspNetCore:RateLimitPermitLimit", "100000");
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, null);

            // Records the integration event next to the sample's own handler.
            services.AddSingleton<RecordedConfirmations>();
            services.AddTransient<IEventHandler<OrderConfirmationRequested>, RecordingConfirmationHandler>();
            _configureServices?.Invoke(services);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        if (_provider == "Postgres")
        {
            await using var scope = Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
        if (_keepAlive is not null)
            await _keepAlive.DisposeAsync();
    }
}

public sealed record RecordedConfirmation(Guid OrderId, bool CommittedWhenHandled);

public sealed class RecordedConfirmations
{
    private readonly ConcurrentQueue<RecordedConfirmation> _items = new();

    public IReadOnlyCollection<RecordedConfirmation> Items => _items;

    public void Add(RecordedConfirmation item) => _items.Enqueue(item);
}

/// <summary>Checks, from a separate scope and connection, that the order is committed when the event arrives.</summary>
public sealed class RecordingConfirmationHandler(RecordedConfirmations recorded, IServiceScopeFactory scopes)
    : IEventHandler<OrderConfirmationRequested>
{
    public async Task HandleAsync(OrderConfirmationRequested @event, CancellationToken ct = default)
    {
        await using var scope = scopes.CreateAsyncScope();
        var committed = await scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Orders
            .AnyAsync(o => o.Id == @event.OrderId, ct);
        recorded.Add(new RecordedConfirmation(@event.OrderId, committed));
    }
}

/// <summary>Fails the first <see cref="OrderPlaced"/> it sees, then succeeds; counts every call.</summary>
public sealed class FlakyOrderPlacedHandler : IDomainEventHandler<OrderPlaced>
{
    private int _calls;

    public int Calls => _calls;

    public Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken) =>
        Interlocked.Increment(ref _calls) == 1
            ? throw new InvalidOperationException("Simulated handler failure.")
            : Task.CompletedTask;
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
