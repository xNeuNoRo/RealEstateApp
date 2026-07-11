using FluentValidation;
using RealEstateApp.Application.Dtos.Property.Requests;

namespace RealEstateApp.Application.Validators.Property;

public sealed class GetPropertyListRequestValidator : AbstractValidator<GetPropertyListRequest>
{
    public GetPropertyListRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        When(
            x => x.PriceMin.HasValue && x.PriceMax.HasValue,
            () =>
            {
                RuleFor(x => x.PriceMax).GreaterThanOrEqualTo(x => x.PriceMin);
            }
        );

        When(
            x => x.SizeMin.HasValue && x.SizeMax.HasValue,
            () =>
            {
                RuleFor(x => x.SizeMax).GreaterThanOrEqualTo(x => x.SizeMin);
            }
        );

        When(x => x.Bedrooms.HasValue, () => RuleFor(x => x.Bedrooms).GreaterThanOrEqualTo(0));

        When(x => x.Bathrooms.HasValue, () => RuleFor(x => x.Bathrooms).GreaterThanOrEqualTo(0));

        When(
            x => !string.IsNullOrWhiteSpace(x.Code),
            () =>
                RuleFor(x => x.Code)
                    .Matches(@"^\d{6}$")
                    .WithMessage("El código de propiedad debe ser de 6 dígitos.")
        );
    }
}
