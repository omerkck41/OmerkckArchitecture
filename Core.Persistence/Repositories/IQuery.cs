namespace Core.Persistence.Repositories;

public interface IQuery<T>
{
    /// <summary>     
    /// Veritabanında sorgulama yapmak için IQueryable döner.
    /// </summary>
    /// <returns>IQueryable</returns>
    IQueryable<T> Query();
}