namespace Kck.FeatureFlags.Abstractions;

public interface IFeatureEvaluator
{
    Task<bool> EvaluateAsync(FeatureDefinition feature, IFeatureContext context, CancellationToken ct = default);
}
