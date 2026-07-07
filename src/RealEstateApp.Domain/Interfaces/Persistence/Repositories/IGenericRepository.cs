using System.Linq.Expressions;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Interfaces.Persistence.Repositories;

/// <summary>
/// Repositorio genérico para operaciones CRUD asíncronas.
/// Expone IQueryable para consultas avanzadas.
/// </summary>
public interface IGenericRepository<T>
    where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(
        QueryOptions<T>? options = null,
        CancellationToken ct = default
    );
    Task<T?> GetFirstOrDefaultAsync(QueryOptions<T> options, CancellationToken ct = default);
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default
    );
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    IQueryable<T> Query();
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}
