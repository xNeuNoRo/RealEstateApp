using FluentValidation;
using RealEstateApp.Application.Dtos.Offers.Requests;

namespace RealEstateApp.Application.Validators.Offers;

public sealed class GetMyOffersRequestValidator : AbstractValidator<GetMyOffersRequest>
{
    public GetMyOffersRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.PropertyId).GreaterThan(0).When(x => x.PropertyId.HasValue);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}
