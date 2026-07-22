using FluentValidation;
using RealEstateApp.Application.Dtos.Catalog.Requests;

namespace RealEstateApp.Application.Validators.Catalog;

public sealed class GetSaleTypeByIdRequestValidator : AbstractValidator<GetSaleTypeByIdRequest>
{
    public GetSaleTypeByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("El ID del tipo de venta debe ser mayor que cero.");
    }
}
