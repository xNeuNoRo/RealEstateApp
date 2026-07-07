using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Interfaces.Services;

/// <summary>
/// Servicio de dominio para invariantes de política de Offer que cruzan límites de agregado.
/// Valida: (1) la propiedad está Disponible, (2) no existe oferta Aceptada para la propiedad,
/// (3) el cliente no tiene oferta Pendiente para esta propiedad.
/// El Application Service llamante usa esto ANTES de Offer.Create().
/// </summary>
public interface IOfferPolicy
{
    /// <returns>Success o error Validation/Conflict.</returns>
    Task<Result> CanCreateOfferAsync(
        int propertyId,
        string clientId,
        CancellationToken ct = default
    );

    /// <returns>Success o error Conflict (solo ofertas Pendiente pueden aceptarse).</returns>
    Task<Result> CanAcceptOfferAsync(int propertyId, int offerId, CancellationToken ct = default);
}
