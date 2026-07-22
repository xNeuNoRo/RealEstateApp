using FluentValidation;
using RealEstateApp.Application.Dtos.Catalog.Requests;

namespace RealEstateApp.Application.Validators.Catalog;

public sealed class DeleteSaleTypeRequestValidator : AbstractValidator<DeleteSaleTypeRequest>
{
    public DeleteSaleTypeRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El ID es requerido.");
    }
}
