using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Domain.Interfaces.Persistence.Repositories;

/// <summary>
/// Repositorio de Offer con consultas de validación de invariantes.
/// </summary>
public interface IOfferRepository : IGenericRepository<Offer>
{
    Task<Offer?> GetAcceptedForPropertyAsync(
        int propertyId,
        QueryOptions<Offer>? options = null,
        CancellationToken ct = default
    );
    Task<Offer?> GetPendingForClientAsync(
        int propertyId,
        string clientId,
        QueryOptions<Offer>? options = null,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<Offer>> GetByPropertyAsync(
        int propertyId,
        QueryOptions<Offer>? options = null,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<Offer>> GetByClientAsync(
        string clientId,
        QueryOptions<Offer>? options = null,
        CancellationToken ct = default
    );
    Task<bool> HasPendingOfferAsync(
        int propertyId,
        string clientId,
        CancellationToken ct = default
    );
    Task<bool> HasAcceptedOfferAsync(int propertyId, CancellationToken ct = default);
    Task<IReadOnlyList<OfferClientSummary>> GetClientSummariesByPropertyAsync(
        int propertyId,
        int skip,
        int take,
        CancellationToken ct = default
    );
    Task<int> CountClientsByPropertyAsync(int propertyId, CancellationToken ct = default);
}
