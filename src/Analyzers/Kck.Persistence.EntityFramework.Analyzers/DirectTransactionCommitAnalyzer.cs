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
        var compilation = context.Compilation;
        var transactionType = compilation.GetTypeByMetadataName(TransactionTypeName);
        if (transactionType is null)
            return; // no EF Core: nothing can be committed

        var state = new CompilationState(
            transactionType,
            compilation.GetTypeByMetadataName(HandlerTypeName),
            compilation.GetTypeByMetadataName(DispatcherTypeName),
            compilation.GetTypeByMetadataName(BuilderTypeName));

        context.RegisterOperationAction(state.OnInvocation, OperationKind.Invocation);
        context.RegisterSymbolAction(state.OnNamedType, SymbolKind.NamedType);
        context.RegisterCompilationEndAction(state.OnCompilationEnd);
    }

    // Per compilation: the direct commits seen, and whether anything shows the compilation uses domain events.
    private sealed class CompilationState(
        INamedTypeSymbol transactionType,
        INamedTypeSymbol? handlerType,
        INamedTypeSymbol? dispatcherType,
        INamedTypeSymbol? builderType)
    {
        private readonly ConcurrentQueue<(Location Location, string Method)> _commits = new();
        private int _usesDomainEvents;

        public void OnInvocation(OperationAnalysisContext context)
        {
            var method = ((IInvocationOperation)context.Operation).TargetMethod;
            if (method.Name is "Commit" or "CommitAsync" && IsOrImplements(method.ContainingType, transactionType))
                _commits.Enqueue((context.Operation.Syntax.GetLocation(), method.Name));
            else if (method.Name is "AddDomainEventHandlers" or "AddDomainEventDispatch"
                     && SymbolEqualityComparer.Default.Equals(method.ContainingType, builderType))
                Interlocked.Exchange(ref _usesDomainEvents, 1);
        }

        public void OnNamedType(SymbolAnalysisContext context)
        {
            if (((INamedTypeSymbol)context.Symbol).AllInterfaces.Any(IsDomainEventContract))
                Interlocked.Exchange(ref _usesDomainEvents, 1);
        }

        public void OnCompilationEnd(CompilationAnalysisContext context)
        {
            if (Volatile.Read(ref _usesDomainEvents) == 0)
                return;

            foreach (var (location, method) in _commits)
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.DirectTransactionCommit, location, method));
        }

        private bool IsDomainEventContract(INamedTypeSymbol @interface) =>
            SymbolEqualityComparer.Default.Equals(@interface.OriginalDefinition, handlerType)
            || SymbolEqualityComparer.Default.Equals(@interface, dispatcherType);

        private static bool IsOrImplements(INamedTypeSymbol type, INamedTypeSymbol interfaceType) =>
            SymbolEqualityComparer.Default.Equals(type, interfaceType)
            || type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceType));
    }
}
