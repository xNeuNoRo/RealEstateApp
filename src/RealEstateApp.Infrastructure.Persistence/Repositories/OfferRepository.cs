using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories;

public sealed class OfferRepository : GenericRepository<Offer>, IOfferRepository
{
    public OfferRepository(AppDbContext context)
        : base(context) { }

    public async Task<Offer?> GetAcceptedForPropertyAsync(
        int propertyId,
        QueryOptions<Offer>? options = null,
        CancellationToken ct = default
    )
    {
        var query = Query()
            .Where(x => x.PropertyId == propertyId && x.Status == OfferStatus.Accepted);
        query = ApplyOptionsToQuery(query, options);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<Offer?> GetPendingForClientAsync(
        int propertyId,
        string clientId,
        QueryOptions<Offer>? options = null,
        CancellationToken ct = default
    )
    {
        var query = Query()
            .Where(x =>
                x.PropertyId == propertyId
                && x.ClientId == clientId
                && x.Status == OfferStatus.Pending
            );
        query = ApplyOptionsToQuery(query, options);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Offer>> GetByPropertyAsync(
        int propertyId,
        QueryOptions<Offer>? options = null,
        CancellationToken ct = default
    )
    {
        var query = Query().Where(x => x.PropertyId == propertyId);
        query = ApplyOptionsToQuery(query, options);
        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Offer>> GetByClientAsync(
        string clientId,
        QueryOptions<Offer>? options = null,
        CancellationToken ct = default
    )
    {
        var query = Query().Where(x => x.ClientId == clientId);
        query = ApplyOptionsToQuery(query, options);
        return await query.ToListAsync(ct);
    }

    public async Task<bool> HasPendingOfferAsync(
        int propertyId,
        string clientId,
        CancellationToken ct = default
    )
    {
        return await _dbSet.AnyAsync(
            x =>
                x.PropertyId == propertyId
                && x.ClientId == clientId
                && x.Status == OfferStatus.Pending,
            ct
        );
    }

    public async Task<bool> HasAcceptedOfferAsync(int propertyId, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(
            x => x.PropertyId == propertyId && x.Status == OfferStatus.Accepted,
            ct
        );
    }

    private static IQueryable<Offer> ApplyOptionsToQuery(
        IQueryable<Offer> query,
        QueryOptions<Offer>? options
    )
    {
        if (options is null)
            return query;

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
