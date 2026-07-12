using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class GetAdminsListRequestValidator : AbstractValidator<GetAdminsListRequest>
{
    public GetAdminsListRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La página debe ser mayor o igual a 1.");
        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("El tamaño de página debe ser mayor o igual a 1.");
    }
}
