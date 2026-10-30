using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Kck.Persistence.EntityFramework.Analyzers;

/// <summary>
/// KCK1002: a synchronous <c>DbContext.SaveChanges</c> call. Not reported inside an override of <c>SaveChanges</c>,
/// which has to call the base implementation.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SyncSaveChangesAnalyzer : DiagnosticAnalyzer
{
    private const string DbContextTypeName = "Microsoft.EntityFrameworkCore.DbContext";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Descriptors.SyncSaveChanges);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var dbContextType = start.Compilation.GetTypeByMetadataName(DbContextTypeName);
            if (dbContextType is null)
                return;

            start.RegisterOperationAction(operationContext =>
            {
                var method = ((IInvocationOperation)operationContext.Operation).TargetMethod;
                // EF's synchronous saves: SaveChanges() and SaveChanges(bool acceptAllChangesOnSuccess).
                if (method.Name != "SaveChanges" || !DerivesFrom(method.ContainingType, dbContextType)
                    || method.Parameters.Length > 1
                    || (method.Parameters.Length == 1 && method.Parameters[0].Type.SpecialType != SpecialType.System_Boolean))
                    return;

                if (operationContext.ContainingSymbol is IMethodSymbol { IsOverride: true, Name: "SaveChanges" })
                    return;

                operationContext.ReportDiagnostic(
                    Diagnostic.Create(Descriptors.SyncSaveChanges, operationContext.Operation.Syntax.GetLocation()));
            }, OperationKind.Invocation);
        });
    }

    private static bool DerivesFrom(INamedTypeSymbol? type, INamedTypeSymbol baseType)
    {
        for (; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type, baseType))
                return true;
        }

        return false;
    }
}
