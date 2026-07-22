using FluentValidation;
using RealEstateApp.Application.Dtos.Property.Requests;

namespace RealEstateApp.Application.Validators.Property;

public sealed class GetPropertyDetailRequestValidator : AbstractValidator<GetPropertyDetailRequest>
{
    public GetPropertyDetailRequestValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0).WithMessage("El ID de propiedad es requerido.");
    }
}
