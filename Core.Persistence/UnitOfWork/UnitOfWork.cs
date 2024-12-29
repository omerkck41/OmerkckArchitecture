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

    public async Task<int> SaveChangesAsync()
    {
        var result = await _context.SaveChangesAsync();

        // Transaction varsa otomatik commit yapılır
        if (_transaction != null)
        {
            await CommitAsync();
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

    public IAsyncRepository<TEntity> Repository<TEntity>() where TEntity : Entity
    {
        var type = typeof(TEntity);

        if (!_repositories.ContainsKey(type))
        {
            // Farklı bir repository türü gerekiyorsa burada kontrol edilebilir
            var repository = CreateRepository<TEntity>();
            _repositories.TryAdd(type, repository);
        }

        return (IAsyncRepository<TEntity>)_repositories[type];
    }

    private EfRepositoryBase<TEntity, TContext> CreateRepository<TEntity>() where TEntity : Entity
    {
        return new EfRepositoryBase<TEntity, TContext>(_context);
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