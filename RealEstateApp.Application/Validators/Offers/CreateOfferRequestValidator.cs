using FluentValidation;
using RealEstateApp.Application.Dtos.Offers.Requests;

namespace RealEstateApp.Application.Validators.Offers;

public sealed class CreateOfferRequestValidator : AbstractValidator<CreateOfferRequest>
{
    public CreateOfferRequestValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0).WithMessage("El ID de propiedad es requerido.");
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor que cero.")
            .LessThanOrEqualTo(100_000_000)
            .WithMessage("El monto no debe exceder 100 millones.");
    }
}
