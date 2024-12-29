using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public interface IQuery<T>
{
    /// <summary>     
    /// Veritabanında sorgulama yapmak için IQueryable döner.
    /// </summary>
    /// <returns>IQueryable</returns>
    IQueryable<T> Query(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
}