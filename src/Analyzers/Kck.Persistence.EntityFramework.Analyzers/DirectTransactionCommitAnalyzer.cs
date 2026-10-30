using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kck.Persistence.EntityFramework.Analyzers;

/// <summary>
/// KCK1001: a transaction committed directly (<c>IDbContextTransaction.Commit</c> / <c>CommitAsync</c>) in a
/// compilation that uses domain events, i.e. implements <c>IDomainEventHandler&lt;T&gt;</c> or
/// <c>IDomainEventDispatcher</c>, or calls <c>AddDomainEventHandlers</c> / <c>AddDomainEventDispatch</c>. Whether it does is known only after the whole
/// compilation is seen, so the commits are collected and reported at compilation end.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DirectTransactionCommitAnalyzer : DiagnosticAnalyzer
{
    internal const string TransactionTypeName = "Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction";
    private const string HandlerTypeName = "Kck.Core.Abstractions.Entities.IDomainEventHandler`1";
    private const string BuilderTypeName = "Kck.Persistence.EntityFramework.KckPersistenceBuilder";
    private const string DispatcherTypeName = "Kck.Persistence.EntityFramework.Interceptors.IDomainEventDispatcher";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptors.DirectTransactionCommit);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var transactionType = context.Compilation.GetTypeByMetadataName(TransactionTypeName);
        if (transactionType is null)
            return; // no EF Core: nothing can be committed

        var handlerType = context.Compilation.GetTypeByMetadataName(HandlerTypeName);
        var builderType = context.Compilation.GetTypeByMetadataName(BuilderTypeName);
        var commits = new ConcurrentQueue<(Location Location, string Method)>();
        var usesDomainEvents = 0;

        context.RegisterOperationAction(operationContext =>
        {
            var method = ((IInvocationOperation)operationContext.Operation).TargetMethod;
            if (method.Name is "Commit" or "CommitAsync" && IsOrImplements(method.ContainingType, transactionType))
                commits.Enqueue((operationContext.Operation.Syntax.GetLocation(), method.Name));
            else if (method.Name is "AddDomainEventHandlers" or "AddDomainEventDispatch"
                     && SymbolEqualityComparer.Default.Equals(method.ContainingType, builderType))
                Interlocked.Exchange(ref usesDomainEvents, 1);
        }, OperationKind.Invocation);

        var dispatcherType = context.Compilation.GetTypeByMetadataName(DispatcherTypeName);
        if (handlerType is not null || dispatcherType is not null)
        {
            context.RegisterSymbolAction(symbolContext =>
            {
                if (((INamedTypeSymbol)symbolContext.Symbol).AllInterfaces
                    .Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, handlerType)
                              || SymbolEqualityComparer.Default.Equals(i, dispatcherType)))
                    Interlocked.Exchange(ref usesDomainEvents, 1);
            }, SymbolKind.NamedType);
        }

        context.RegisterCompilationEndAction(endContext =>
        {
            if (Volatile.Read(ref usesDomainEvents) == 0)
                return;

            foreach (var (location, method) in commits)
                endContext.ReportDiagnostic(Diagnostic.Create(Descriptors.DirectTransactionCommit, location, method));
        });
    }

    private static bool IsOrImplements(INamedTypeSymbol type, INamedTypeSymbol interfaceType) =>
        SymbolEqualityComparer.Default.Equals(type, interfaceType)
        || type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceType));
}
