using FluentValidation;
using RealEstateApp.Application.Dtos.Auth.Requests;

namespace RealEstateApp.Application.Validators.Auth;

public sealed class ResendActivationRequestValidator : AbstractValidator<ResendActivationRequest>
{
    public ResendActivationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es requerido.")
            .EmailAddress()
            .WithMessage("Debe ingresar un correo electrónico válido.");
    }
}
