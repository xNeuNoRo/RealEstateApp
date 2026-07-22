using FluentValidation;
using RealEstateApp.Application.Dtos.Catalog.Requests;

namespace RealEstateApp.Application.Validators.Catalog;

public sealed class UpdateImprovementRequestValidator : AbstractValidator<UpdateImprovementRequest>
{
    public UpdateImprovementRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El ID es requerido.");
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre es requerido.")
            .MaximumLength(100)
            .WithMessage("El nombre no debe exceder 100 caracteres.");
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("La descripción es requerida.")
            .MaximumLength(300)
            .WithMessage("La descripción no debe exceder 300 caracteres.");
    }
}
