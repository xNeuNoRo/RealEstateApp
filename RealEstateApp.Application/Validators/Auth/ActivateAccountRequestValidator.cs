using FluentValidation;
using RealEstateApp.Application.Dtos.Auth.Requests;

namespace RealEstateApp.Application.Validators.Auth;

public sealed class ActivateAccountRequestValidator : AbstractValidator<ActivateAccountRequest>
{
    public ActivateAccountRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("El identificador de usuario es requerido.");

        RuleFor(x => x.Token).NotEmpty().WithMessage("El token de activación es requerido.");
    }
}
