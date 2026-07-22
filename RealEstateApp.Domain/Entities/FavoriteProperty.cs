using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa una propiedad favorita de un cliente.
/// </summary>
public class FavoriteProperty : AggregateRoot
{
    public string ClientId { get; private set; } = null!;
    public int PropertyId { get; private set; }

    public Property Property { get; private set; } = null!;

    private FavoriteProperty() { }

    public static Result<FavoriteProperty> Create(string clientId, int propertyId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return Result.Failure<FavoriteProperty>(
                Error.Validation("Favorite.ClientId", "El cliente es requerido.")
            );
        if (propertyId <= 0)
            return Result.Failure<FavoriteProperty>(
                Error.Validation("Favorite.PropertyId", "La propiedad es requerida.")
            );

        return Result.Success(
            new FavoriteProperty { ClientId = clientId, PropertyId = propertyId }
        );
    }
}
