using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Domain.Interfaces.Services;

namespace RealEstateApp.Infrastructure.Persistence.Services;

/// <summary>
/// Validación de políticas de dominio para Offers que cruzan límites de agregados.
/// Consulta repositorios para verificar invariantes antes de crear/aceptar una oferta.
/// </summary>
public sealed class OfferPolicy : IOfferPolicy
{
    private readonly IPropertyRepository _propertyRepo;
    private readonly IOfferRepository _offerRepo;

    public OfferPolicy(IPropertyRepository propertyRepo, IOfferRepository offerRepo)
    {
        _propertyRepo = propertyRepo;
        _offerRepo = offerRepo;
    }

    public async Task<Result> CanCreateOfferAsync(
        int propertyId,
        string clientId,
        CancellationToken ct = default
    )
    {
        var property = await _propertyRepo.GetByIdAsync(propertyId, ct);
        if (property is null)
            return Result.Failure(
                Error.NotFound("Offer.PropertyNotFound", "La propiedad no existe.")
            );

        if (property.Status != PropertyStatus.Available)
            return Result.Failure(
                Error.Conflict(
                    "Offer.PropertyNotAvailable",
                    "Esta propiedad no está disponible para ofertas."
                )
            );

        bool hasAccepted = await _offerRepo.HasAcceptedOfferAsync(propertyId, ct);
        if (hasAccepted)
            return Result.Failure(
                Error.Conflict(
                    "Offer.PropertyHasAcceptedOffer",
                    "La propiedad ya tiene una oferta aceptada."
                )
            );

        bool hasPending = await _offerRepo.HasPendingOfferAsync(propertyId, clientId, ct);
        if (hasPending)
            return Result.Failure(
                Error.Conflict(
                    "Offer.ClientHasPending",
                    "Ya tienes una oferta pendiente para esta propiedad."
                )
            );

        return Result.Success();
    }

    public async Task<Result> CanAcceptOfferAsync(
        int propertyId,
        int offerId,
        CancellationToken ct = default
    )
    {
        var property = await _propertyRepo.GetByIdAsync(propertyId, ct);
        if (property is null)
            return Result.Failure(
                Error.NotFound("Offer.PropertyNotFound", "La propiedad no existe.")
            );

        if (property.Status != PropertyStatus.Available)
            return Result.Failure(
                Error.Conflict(
                    "Offer.PropertyNotAvailable",
                    "La propiedad no está disponible. Ya tiene una oferta aceptada o fue vendida."
                )
            );

        return Result.Success();
    }
}
