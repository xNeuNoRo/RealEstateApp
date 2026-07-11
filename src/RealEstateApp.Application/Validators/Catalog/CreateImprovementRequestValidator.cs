using FluentValidation;
using RealEstateApp.Application.Dtos.Catalog.Requests;

namespace RealEstateApp.Application.Validators.Catalog;

public sealed class CreateImprovementRequestValidator : AbstractValidator<CreateImprovementRequest>
{
    public CreateImprovementRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre es requerido.")
            .MaximumLength(80)
            .WithMessage("El nombre no debe exceder 80 caracteres.");
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("La descripción es requerida.")
            .MaximumLength(80)
            .WithMessage("La descripción no debe exceder 80 caracteres.");
    }
}
