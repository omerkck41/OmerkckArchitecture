using Core.Persistence.Entities;
using Core.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace Core.Persistence.UnitOfWork;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context = context;
    private IDbContextTransaction? _transaction;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public async Task BeginTransactionAsync()
    {
        _transaction ??= await _context.Database.BeginTransactionAsync();
    }

    public async Task<int> SaveChangesAsync(bool autoCommitTransaction = false)
    {
        var result = await _context.SaveChangesAsync();

        if (autoCommitTransaction && _transaction != null)
        {
            try
            {
                await CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        return result;
    }

    public async Task<int> SaveChangesWithoutTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("Cannot save changes without committing the active transaction.");

        return await _context.SaveChangesAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction has been started.");

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction has been started.");

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public IAsyncRepository<TEntity, TId> Repository<TEntity, TId>() where TEntity : Entity<TId>
    {
        var type = typeof(TEntity);

        if (!_repositories.ContainsKey(type))
        {
            // Farklı bir repository türü gerekiyorsa burada kontrol edilebilir
            var repository = CreateRepository<TEntity, TId>();
            _repositories.TryAdd(type, repository);
        }

        return (IAsyncRepository<TEntity, TId>)_repositories[type];
    }

    private EfRepositoryBase<TEntity, TId, TContext> CreateRepository<TEntity, TId>() where TEntity : Entity<TId>
    {
        return new EfRepositoryBase<TEntity, TId, TContext>(_context);
    }


    public void Dispose()
    {
        // Managed kaynakları serbest bırak
        _transaction?.Dispose();
        _context.Dispose();

        // Garbage collector'ün finalize çağrısını önler
        GC.SuppressFinalize(this);
    }
}