namespace Kck.FeatureFlags.Abstractions;

/// <summary>
/// Evaluates whether a given feature should be considered active for a specific context, implementing the filter logic for a <see cref="FeatureFilterType"/>.
/// </summary>
public interface IFeatureEvaluator
{
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="feature"/> should be enabled for the supplied <paramref name="context"/>; otherwise <see langword="false"/>.
    /// </summary>
    Task<bool> EvaluateAsync(FeatureDefinition feature, IFeatureContext context, CancellationToken ct = default);
}
