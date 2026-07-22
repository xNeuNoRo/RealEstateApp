using FluentValidation;
using RealEstateApp.Application.Dtos.Property.Requests;

namespace RealEstateApp.Application.Validators.Property;

public sealed class SearchPropertyByCodeRequestValidator
    : AbstractValidator<SearchPropertyByCodeRequest>
{
    public SearchPropertyByCodeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("El código de propiedad es requerido.")
            .Matches(@"^\d{6}$")
            .WithMessage("El código de propiedad debe ser de 6 dígitos.");
    }
}
