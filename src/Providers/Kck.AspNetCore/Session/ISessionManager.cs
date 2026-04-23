using Microsoft.AspNetCore.Http;

namespace Kck.AspNetCore.Session;

public interface ISessionManager
{
    void Set<T>(ISession session, string key, T value);
    T? Get<T>(ISession session, string key);
    void Remove(ISession session, string key);
    void Clear(ISession session);
    bool Exists(ISession session, string key);
}
