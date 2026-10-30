using Microsoft.CodeAnalysis;

namespace Kck.Persistence.EntityFramework.Analyzers;

/// <summary>The diagnostics of Kck.Persistence.EntityFramework (ADR-0031). Ids are KCK1xxx; KCK0xxx is for [Obsolete].</summary>
internal static class Descriptors
{
    private const string Category = "Kck.Persistence";
    private const string HelpLinkFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/reference/diagnostics/{0}.md";

    public static readonly DiagnosticDescriptor DirectTransactionCommit = new(
        id: "KCK1001",
        title: "Commit through IUnitOfWork so domain events are dispatched",
        messageFormat: "'{0}' commits the transaction directly: domain events raised in it wait for a later SaveChanges and are lost if none follows. Commit through IUnitOfWork.CommitAsync.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Domain events saved inside a transaction are dispatched after IUnitOfWork commits it. A transaction committed directly leaves them waiting for the context's next SaveChanges.",
        helpLinkUri: string.Format(System.Globalization.CultureInfo.InvariantCulture, HelpLinkFormat, "KCK1001"),
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor SyncSaveChanges = new(
        id: "KCK1002",
        title: "Prefer SaveChangesAsync",
        messageFormat: "SaveChanges blocks the calling thread; domain event dispatch then runs on a thread-pool thread. Use SaveChangesAsync.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Kck dispatches domain events asynchronously. A synchronous save blocks while dispatch runs on the thread pool, and a warning is logged at run time.",
        helpLinkUri: string.Format(System.Globalization.CultureInfo.InvariantCulture, HelpLinkFormat, "KCK1002"));
}
