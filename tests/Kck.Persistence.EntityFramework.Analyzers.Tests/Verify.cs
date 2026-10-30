using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kck.Persistence.EntityFramework.Analyzers.Tests;

/// <summary>A diagnostic as a test expects it: id and the source text it is reported on.</summary>
internal sealed record Expected(string Id, string Text);

/// <summary>
/// Runs the analyzers through Roslyn directly, over test code compiled against the assemblies of this test host
/// (.NET 10, EF Core, Kck): what an application that installed Kck.Persistence.EntityFramework references.
/// </summary>
internal static class Verify
{
    private static readonly ImmutableArray<MetadataReference> References =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Where(path => !Path.GetFileName(path).StartsWith("Kck.Persistence.EntityFramework.Analyzers", StringComparison.Ordinal))
            .Select(path => MetadataReference.CreateFromFile(path)),
    ];

    /// <summary>Asserts the analyzer reports exactly <paramref name="expected"/> (in source order).</summary>
    public static async Task AnalyzerAsync(DiagnosticAnalyzer analyzer, string source, params Expected[] expected)
    {
        var compilation = CSharpCompilation.Create(
            "Test",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();

        var diagnostics = await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync();

        diagnostics.OrderBy(d => d.Location.SourceSpan.Start)
            .Select(d => new Expected(d.Id, d.Location.SourceTree!.GetText().ToString(d.Location.SourceSpan)))
            .Should().Equal(expected);
    }
}
