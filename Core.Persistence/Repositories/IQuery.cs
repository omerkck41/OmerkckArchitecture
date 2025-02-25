using System.Linq.Expressions;

namespace Core.Persistence.Repositories;

public interface IQuery<T>
{
    /// <summary>
    /// Veritabanında sorgulama yapmak için IQueryable döner.
    /// </summary>
    /// <param name="filter">Filtre uygulanacak koşul.</param>
    /// <param name="orderBy">Sıralama fonksiyonu.</param>
    /// <param name="withDeleted">Silinmiş kayıtları getirip getirmeyeceğini belirler.</param>
    /// <param name="enableTracking">Entity Framework değişiklik izleme mekanizmasını aktif edip etmeyeceğini belirler.</param>
    /// <returns>IQueryable</returns>
    IQueryable<T> Query(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = false);
}