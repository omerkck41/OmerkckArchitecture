namespace Kck.Persistence.EntityFramework.Analyzers.Tests;

public sealed class SyncSaveChangesAnalyzerTests
{
    private const string Usings = """
        using System.Threading;
        using System.Threading.Tasks;
        using Microsoft.EntityFrameworkCore;

        """;

    [Fact]
    public async Task SyncSaveChanges_ReportsKCK1002()
    {
        var source = Usings + """
            public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);

            public sealed class Service(AppDbContext db, DbContext other)
            {
                public int Save() => db.SaveChanges();

                public int SaveAccepting() => other.SaveChanges(acceptAllChangesOnSuccess: true);
            }
            """;

        await Verify.AnalyzerAsync(new SyncSaveChangesAnalyzer(), source,
            new Expected("KCK1002", "db.SaveChanges()"),
            new Expected("KCK1002", "other.SaveChanges(acceptAllChangesOnSuccess: true)"));
    }

    [Fact]
    public async Task SaveChangesAsync_NoDiagnostic()
    {
        var source = Usings + """
            public sealed class Service(DbContext db)
            {
                public Task<int> SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
            }
            """;

        await Verify.AnalyzerAsync(new SyncSaveChangesAnalyzer(), source);
    }

    [Fact]
    public async Task BaseCallInsideSaveChangesOverride_NoDiagnostic()
    {
        // An override of SaveChanges has to call the base implementation; that is not a choice the user can change.
        var source = Usings + """
            public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
            {
                public override int SaveChanges(bool acceptAllChangesOnSuccess) => base.SaveChanges(acceptAllChangesOnSuccess);
            }
            """;

        await Verify.AnalyzerAsync(new SyncSaveChangesAnalyzer(), source);
    }

    [Fact]
    public async Task OwnSaveChangesOverload_NoDiagnostic()
    {
        // Only DbContext.SaveChanges() and SaveChanges(bool) are EF's synchronous saves.
        var source = Usings + """
            public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
            {
                public int SaveChanges(string user) => 0;
            }

            public sealed class Service(AppDbContext db)
            {
                public int Save() => db.SaveChanges("alice");
            }
            """;

        await Verify.AnalyzerAsync(new SyncSaveChangesAnalyzer(), source);
    }

    [Fact]
    public async Task OtherSaveChangesMethod_NoDiagnostic()
    {
        // A method named SaveChanges on a type that is not a DbContext.
        var source = Usings + """
            public sealed class Session
            {
                public int SaveChanges() => 0;
            }

            public sealed class Service(Session session)
            {
                public int Save() => session.SaveChanges();
            }
            """;

        await Verify.AnalyzerAsync(new SyncSaveChangesAnalyzer(), source);
    }
}
