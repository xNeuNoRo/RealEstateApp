using System.Data;

namespace RealEstateApp.Domain.Interfaces.Persistence;

/// <summary>
/// Unit of Work para manejo de transacciones explícitas y persistencia atómica.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default
    );

    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
