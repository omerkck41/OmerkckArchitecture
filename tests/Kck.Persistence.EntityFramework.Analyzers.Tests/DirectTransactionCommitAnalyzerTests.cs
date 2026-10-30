namespace Kck.Persistence.EntityFramework.Analyzers.Tests;

public sealed class DirectTransactionCommitAnalyzerTests
{
    private const string Usings = """
        using System;
        using System.Threading;
        using System.Threading.Tasks;
        using Kck.Core.Abstractions.Entities;
        using Kck.Persistence.Abstractions.UnitOfWork;
        using Microsoft.EntityFrameworkCore;
        using Microsoft.EntityFrameworkCore.Storage;

        """;

    // Makes the compilation one that uses domain events.
    private const string Handler = """

        public sealed record Placed(Guid Id) : IDomainEvent
        {
            public Guid EventId { get; } = Guid.NewGuid();
            public DateTime OccurredOn { get; } = DateTime.UtcNow;
        }

        public sealed class PlacedHandler : IDomainEventHandler<Placed>
        {
            public Task HandleAsync(Placed domainEvent, CancellationToken cancellationToken) => Task.CompletedTask;
        }

        """;

    [Fact]
    public async Task DirectCommit_WithDomainEvents_ReportsKCK1001()
    {
        var source = Usings + """
            public sealed class Service(DbContext db)
            {
                public async Task RunAsync(CancellationToken ct)
                {
                    await using var transaction = await db.Database.BeginTransactionAsync(ct);
                    await db.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }

                public void Run(IDbContextTransaction transaction) => transaction.Commit();
            }
            """ + Handler;

        await Verify.AnalyzerAsync(new DirectTransactionCommitAnalyzer(), source,
            new Expected("KCK1001", "transaction.CommitAsync(ct)"),
            new Expected("KCK1001", "transaction.Commit()"));
    }

    [Fact]
    public async Task DirectCommit_WithAddDomainEventHandlersCall_ReportsKCK1001()
    {
        var source = Usings + """
            using Kck.Persistence.EntityFramework;

            public static class Setup
            {
                public static void Configure(KckPersistenceBuilder persistence) =>
                    persistence.AddDomainEventHandlers(typeof(Setup).Assembly);

                public static Task CommitAsync(IDbContextTransaction transaction) => transaction.CommitAsync();
            }
            """;

        await Verify.AnalyzerAsync(new DirectTransactionCommitAnalyzer(), source,
            new Expected("KCK1001", "transaction.CommitAsync()"));
    }

    [Fact]
    public async Task DirectCommit_WithOwnDispatcher_ReportsKCK1001()
    {
        // A project that dispatches through its own IDomainEventDispatcher (registered in another project).
        var source = Usings + """
            using System.Collections.Generic;
            using Kck.Persistence.EntityFramework.Interceptors;

            public sealed class MediatorDispatcher : IDomainEventDispatcher
            {
                public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default) => Task.CompletedTask;
            }

            public static class Repository
            {
                public static Task CommitAsync(IDbContextTransaction transaction) => transaction.CommitAsync();
            }
            """;

        await Verify.AnalyzerAsync(new DirectTransactionCommitAnalyzer(), source,
            new Expected("KCK1001", "transaction.CommitAsync()"));
    }

    [Fact]
    public async Task DirectCommit_WithoutDomainEvents_NoDiagnostic()
    {
        var source = Usings + """
            public sealed class Service(DbContext db)
            {
                public async Task RunAsync(CancellationToken ct)
                {
                    await using var transaction = await db.Database.BeginTransactionAsync(ct);
                    await transaction.CommitAsync(ct);
                }
            }
            """;

        await Verify.AnalyzerAsync(new DirectTransactionCommitAnalyzer(), source);
    }

    [Fact]
    public async Task UnitOfWorkCommit_NoDiagnostic()
    {
        var source = Usings + """
            public sealed class Service(IUnitOfWork unitOfWork)
            {
                public async Task RunAsync(CancellationToken ct)
                {
                    await unitOfWork.BeginTransactionAsync(ct);
                    await unitOfWork.SaveChangesAsync(cancellationToken: ct);
                    await unitOfWork.CommitAsync(ct);
                }
            }
            """ + Handler;

        await Verify.AnalyzerAsync(new DirectTransactionCommitAnalyzer(), source);
    }
}
