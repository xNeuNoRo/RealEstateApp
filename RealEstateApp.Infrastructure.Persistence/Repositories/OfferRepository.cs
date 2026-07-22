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

    public async Task<IReadOnlyList<OfferClientSummary>> GetClientSummariesByPropertyAsync(
        int propertyId,
        int skip,
        int take,
        CancellationToken ct = default
    )
    {
        var offers = await _dbSet
            .AsNoTracking()
            .Where(offer => offer.PropertyId == propertyId)
            .Select(offer => new
            {
                offer.ClientId,
                offer.Amount,
                offer.Status,
                offer.CreatedAt,
                offer.Id
            })
            .ToListAsync(ct);

        return offers
            .GroupBy(o => o.ClientId)
            .Select(g =>
            {
                var last = g.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id).First();
                return new OfferClientSummary(
                    g.Key,
                    g.Count(),
                    last.Amount,
                    last.Status,
                    g.Max(o => o.CreatedAt)
                );
            })
            .OrderByDescending(s => s.LastCreatedAt)
            .ThenBy(s => s.ClientId)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public Task<int> CountClientsByPropertyAsync(
        int propertyId,
        CancellationToken ct = default
    ) =>
        _dbSet
            .AsNoTracking()
            .Where(offer => offer.PropertyId == propertyId)
            .Select(offer => offer.ClientId)
            .Distinct()
            .CountAsync(ct);

    private static IQueryable<Offer> ApplyOptionsToQuery(
        IQueryable<Offer> query,
        QueryOptions<Offer>? options
    )
    {
        if (options is null)
            return query;

        if (options.UseSplitQuery)
            query = query.AsSplitQuery();

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
