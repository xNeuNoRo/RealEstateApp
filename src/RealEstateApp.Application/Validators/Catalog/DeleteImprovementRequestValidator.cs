using FluentValidation;
using RealEstateApp.Application.Dtos.Catalog.Requests;

namespace RealEstateApp.Application.Validators.Catalog;

public sealed class DeleteImprovementRequestValidator : AbstractValidator<DeleteImprovementRequest>
{
    public DeleteImprovementRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El ID es requerido.");
    }
}
