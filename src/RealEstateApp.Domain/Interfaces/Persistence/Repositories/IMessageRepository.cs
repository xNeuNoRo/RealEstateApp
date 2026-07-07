using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Domain.Interfaces.Persistence.Repositories;

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<IReadOnlyList<Message>> GetByPropertyAsync(
        int propertyId,
        QueryOptions<Message>? options = null,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<Message>> GetConversationAsync(
        int propertyId,
        string clientId,
        string agentId,
        QueryOptions<Message>? options = null,
        CancellationToken ct = default
    );
}
