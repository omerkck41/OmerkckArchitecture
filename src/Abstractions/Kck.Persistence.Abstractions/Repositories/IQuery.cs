using System.Linq.Expressions;

namespace Kck.Persistence.Abstractions.Repositories;

public interface IQuery<T>
{
    IQueryable<T> Query(Expression<Func<T, bool>>? filter = null,
                        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                        bool withDeleted = false,
                        bool enableTracking = false);
}
