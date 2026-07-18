using FluentValidation;
using RealEstateApp.Application.Dtos.Catalog.Requests;

namespace RealEstateApp.Application.Validators.Catalog;

public sealed class GetImprovementByIdRequestValidator
    : AbstractValidator<GetImprovementByIdRequest>
{
    public GetImprovementByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El ID de la mejora debe ser mayor que cero.");
    }
}
