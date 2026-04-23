namespace Kck.Observability.Abstractions;

public interface ICounter
{
    void Increment(double value = 1, params KeyValuePair<string, object?>[] tags);
}

public interface IHistogram
{
    void Record(double value, params KeyValuePair<string, object?>[] tags);
}

public interface IGauge
{
    void Set(double value, params KeyValuePair<string, object?>[] tags);
}
