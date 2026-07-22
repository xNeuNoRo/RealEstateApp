using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories;

public sealed class FavoritePropertyRepository
    : GenericRepository<FavoriteProperty>,
        IFavoritePropertyRepository
{
    public FavoritePropertyRepository(AppDbContext context)
        : base(context) { }

    public async Task<FavoriteProperty?> GetAsync(
        string clientId,
        int propertyId,
        CancellationToken ct = default
    )
    {
        return await _dbSet.FirstOrDefaultAsync(
            x => x.ClientId == clientId && x.PropertyId == propertyId,
            ct
        );
    }

    public async Task<IReadOnlyList<FavoriteProperty>> GetByUserAsync(
        string clientId,
        QueryOptions<FavoriteProperty>? options = null,
        CancellationToken ct = default
    )
    {
        var query = Query().Where(x => x.ClientId == clientId);
        query = ApplyOptionsToQuery(query, options);
        return await query.ToListAsync(ct);
    }

    public async Task<bool> IsFavoritedAsync(
        string clientId,
        int propertyId,
        CancellationToken ct = default
    )
    {
        return await _dbSet.AnyAsync(x => x.ClientId == clientId && x.PropertyId == propertyId, ct);
    }

    public async Task<IReadOnlySet<int>> GetPropertyIdsAsync(
        string clientId,
        IReadOnlyCollection<int> propertyIds,
        CancellationToken ct = default
    )
    {
        if (propertyIds.Count == 0)
            return new HashSet<int>();

        var ids = await _dbSet
            .Where(x => x.ClientId == clientId && propertyIds.Contains(x.PropertyId))
            .Select(x => x.PropertyId)
            .ToListAsync(ct);

        return ids.ToHashSet();
    }

    public async Task<int> CountByPropertyAsync(int propertyId, CancellationToken ct = default)
    {
        return await _dbSet.CountAsync(x => x.PropertyId == propertyId, ct);
    }

    private static IQueryable<FavoriteProperty> ApplyOptionsToQuery(
        IQueryable<FavoriteProperty> query,
        QueryOptions<FavoriteProperty>? options
    )
    {
        if (options is null)
            return query;

        if (options.Filter is not null)
            query = query.Where(options.Filter);

        if (options.UseSplitQuery)
            query = query.AsSplitQuery();

        foreach (var include in options.Includes)
            query = query.Include(include);

        if (options.OrderBy is not null)
            query = options.OrderBy(query);

        if (options.Skip.HasValue)
            query = query.Skip(options.Skip.Value);

        if (options.Take.HasValue)
            query = query.Take(options.Take.Value);

        return query;
    }
}
