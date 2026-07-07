using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Domain.Interfaces.Persistence.Repositories;

/// <summary>
/// Repositorio de FavoriteProperty.
/// </summary>
public interface IFavoritePropertyRepository : IGenericRepository<FavoriteProperty>
{
    Task<FavoriteProperty?> GetAsync(
        string clientId,
        int propertyId,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<FavoriteProperty>> GetByUserAsync(
        string clientId,
        QueryOptions<FavoriteProperty>? options = null,
        CancellationToken ct = default
    );
    Task<bool> IsFavoritedAsync(string clientId, int propertyId, CancellationToken ct = default);
    Task<int> CountByPropertyAsync(int propertyId, CancellationToken ct = default);
}
