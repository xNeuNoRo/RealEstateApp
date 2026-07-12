using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class ToggleAdminActiveRequestValidator : AbstractValidator<ToggleAdminActiveRequest>
{
    public ToggleAdminActiveRequestValidator()
    {
        RuleFor(x => x.AdminId).NotEmpty().WithMessage("El administrador es requerido.");
    }
}
