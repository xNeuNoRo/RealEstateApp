using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class GetAdminByIdRequestValidator : AbstractValidator<GetAdminByIdRequest>
{
    public GetAdminByIdRequestValidator()
    {
        RuleFor(x => x.AdminId).NotEmpty().WithMessage("El ID del administrador es requerido.");
    }
}
