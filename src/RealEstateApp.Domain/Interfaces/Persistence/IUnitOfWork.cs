using System.Data;

namespace RealEstateApp.Domain.Interfaces.Persistence;

/// <summary>
/// Unit of Work para manejo de transacciones explícitas y persistencia atómica.
/// Soportes transacciones anidadas reutilizando la activa si ya existe.
/// Necesario para operaciones cross-agregado como AcceptOffer.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync(
        CancellationToken ct = default,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
    );

    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
