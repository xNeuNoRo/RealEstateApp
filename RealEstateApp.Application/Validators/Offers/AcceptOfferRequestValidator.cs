using FluentValidation;
using RealEstateApp.Application.Dtos.Offers.Requests;

namespace RealEstateApp.Application.Validators.Offers;

public sealed class AcceptOfferRequestValidator : AbstractValidator<AcceptOfferRequest>
{
    public AcceptOfferRequestValidator()
    {
        RuleFor(x => x.OfferId).GreaterThan(0).WithMessage("El ID de oferta es requerido.");
    }
}
