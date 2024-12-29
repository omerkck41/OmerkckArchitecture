using Core.Persistence.Entities;
using Core.Persistence.Repositories;

namespace Core.Persistence.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Bir transaction başlatır.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Tüm değişiklikleri kaydeder.
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Transaction'ı onaylar.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Transaction'ı geri alır.
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// İlgili repository'yi döner.
    /// </summary>
    IAsyncRepository<T> Repository<T>() where T : Entity;
    new void Dispose();
}