using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public bool HasActiveTransaction => _transaction is not null;

    public async Task BeginTransactionAsync(
        CancellationToken ct = default,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
    )
    {
        if (HasActiveTransaction)
            return;

        _transaction = await _context.Database.BeginTransactionAsync(isolationLevel, ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        try
        {
            await _context.SaveChangesAsync(ct);

            if (_transaction is not null)
                await _transaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackAsync(ct);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await DisposeTransactionAsync();
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        DisposeTransaction();
        GC.SuppressFinalize(this);
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    private void DisposeTransaction()
    {
        if (_transaction is not null)
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }
}
