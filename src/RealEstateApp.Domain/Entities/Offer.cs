using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Events;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa una oferta realizada por un cliente para una propiedad.
/// </summary>
public class Offer : AggregateRoot
{
    public int PropertyId { get; private set; }
    public string ClientId { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public OfferStatus Status { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }

    private Offer() { }

    public static Result<Offer> Create(int propertyId, string clientId, decimal amount)
    {
        if (propertyId <= 0)
            return Result.Failure<Offer>(
                Error.Validation("Offer.PropertyId", "La propiedad es requerida.")
            );
        if (string.IsNullOrWhiteSpace(clientId))
            return Result.Failure<Offer>(
                Error.Validation("Offer.ClientId", "El cliente es requerido.")
            );
        if (amount <= 0)
            return Result.Failure<Offer>(
                Error.Validation("Offer.Amount", "El monto debe ser mayor que cero.")
            );

        return Result.Success(
            new Offer
            {
                PropertyId = propertyId,
                ClientId = clientId,
                Amount = amount,
                Status = OfferStatus.Pending,
            }
        );
    }

    public Result Accept()
    {
        if (Status != OfferStatus.Pending)
            return Result.Failure(
                Error.Conflict(
                    "Offer.NotPendiente",
                    "Solo ofertas pendientes pueden ser aceptadas."
                )
            );

        Status = OfferStatus.Accepted;
        RespondedAt = DateTimeOffset.UtcNow;
        RaiseEvent(new OfferAcceptedEvent(Id, PropertyId, ClientId, Amount, DateTimeOffset.UtcNow));
        Touch();
        return Result.Success();
    }

    public Result Reject()
    {
        if (Status != OfferStatus.Pending)
            return Result.Failure(
                Error.Conflict(
                    "Offer.NotPendiente",
                    "Solo ofertas pendientes pueden ser rechazadas."
                )
            );

        Status = OfferStatus.Rejected;
        RespondedAt = DateTimeOffset.UtcNow;
        Touch();
        return Result.Success();
    }
}
