using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Interfaces.Events;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDomainEventDispatcher _dispatcher;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context, IDomainEventDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public bool HasActiveTransaction => _transaction is not null;

    public async Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default
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

            await DispatchDomainEventsAsync(ct);

            await _context.SaveChangesAsync(ct);

            if (_transaction is not null)
                await _transaction.CommitAsync(ct);
        }
        catch
        {
            try
            {
                await RollbackAsync(ct);
            }
            catch
            {
                /* No arrojamos nada para preservar la excepcion original. */
            }
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

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var aggregates = _context
            .ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            // Copiamos los eventos a una lista para evitar modificar la colección mientras iteramos.
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            // Usamos reflexión para invocar el método genérico DispatchAsync con el tipo de evento correcto.
            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                var method = typeof(IDomainEventDispatcher)
                    .GetMethod(nameof(IDomainEventDispatcher.DispatchAsync))!
                    .MakeGenericMethod(eventType);

                // Invocamos el método genérico y esperamos su resultado.
                await (Task)method.Invoke(_dispatcher, [@event, ct])!;
            }
        }
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
