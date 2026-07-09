using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories;

public sealed class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(AppDbContext context)
        : base(context) { }

    public async Task<IReadOnlyList<Message>> GetByPropertyAsync(
        int propertyId,
        QueryOptions<Message>? options = null,
        CancellationToken ct = default
    )
    {
        var query = Query().Where(x => x.PropertyId == propertyId);
        query = ApplyOptionsToQuery(query, options);
        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Message>> GetConversationAsync(
        int propertyId,
        string clientId,
        string agentId,
        QueryOptions<Message>? options = null,
        CancellationToken ct = default
    )
    {
        var query = Query()
            .Where(x =>
                x.PropertyId == propertyId && x.ClientId == clientId && x.AgentId == agentId
            );
        query = ApplyOptionsToQuery(query, options);
        return await query.ToListAsync(ct);
    }

    private static IQueryable<Message> ApplyOptionsToQuery(
        IQueryable<Message> query,
        QueryOptions<Message>? options
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
