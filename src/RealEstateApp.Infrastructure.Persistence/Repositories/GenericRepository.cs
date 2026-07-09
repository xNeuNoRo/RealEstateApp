using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T>
    where T : BaseEntity<int>
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(
        int id,
        CancellationToken ct = default,
        params Expression<Func<T, object>>[] includes
    )
    {
        if (includes.Length == 0)
            return await _dbSet.FindAsync([id], ct);

        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(
        QueryOptions<T>? options = null,
        CancellationToken ct = default
    )
    {
        return await ApplyOptions(options).ToListAsync(ct);
    }

    public virtual async Task<T?> GetFirstOrDefaultAsync(
        QueryOptions<T> options,
        CancellationToken ct = default
    )
    {
        return await ApplyOptions(options).FirstOrDefaultAsync(ct);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default
    )
    {
        return predicate is null
            ? await _dbSet.CountAsync(ct)
            : await _dbSet.CountAsync(predicate, ct);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default
    )
    {
        return await _dbSet.AnyAsync(predicate, ct);
    }

    public virtual IQueryable<T> Query() => _dbSet.AsNoTracking();

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Delete(T entity) => _dbSet.Remove(entity);

    protected IQueryable<T> ApplyOptions(QueryOptions<T>? options)
    {
        IQueryable<T> query = _dbSet;

        if (options is null)
            return query.AsNoTracking();

        if (!options.IsTracking)
            query = query.AsNoTracking();

        foreach (var include in options.Includes)
            query = query.Include(include);

        if (options.Filter is not null)
            query = query.Where(options.Filter);

        if (options.OrderBy is not null)
            query = options.OrderBy(query);

        if (options.Skip.HasValue)
            query = query.Skip(options.Skip.Value);

        if (options.Take.HasValue)
            query = query.Take(options.Take.Value);

        return query;
    }
}
