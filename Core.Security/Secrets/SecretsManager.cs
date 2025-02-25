using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Security.Secrets;

public class SecretsManager
{
    private readonly Dictionary<string, string> _secrets = [];

    public void AddSecret(string key, string value)
    {
        _secrets[key] = value;
    }

    public string GetSecret(string key)
    {
        return _secrets.ContainsKey(key) ? _secrets[key] : throw new CustomException($"Secret with key '{key}' not found.");
    }
}