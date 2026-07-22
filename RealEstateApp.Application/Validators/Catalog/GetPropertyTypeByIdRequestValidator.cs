using FluentValidation;
using RealEstateApp.Application.Dtos.Catalog.Requests;

namespace RealEstateApp.Application.Validators.Catalog;

public sealed class GetPropertyTypeByIdRequestValidator
    : AbstractValidator<GetPropertyTypeByIdRequest>
{
    public GetPropertyTypeByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El ID del tipo de propiedad debe ser mayor que cero.");
    }
}
