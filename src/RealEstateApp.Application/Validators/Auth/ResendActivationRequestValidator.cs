using FluentValidation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Application.Validators.Auth;

public sealed class ResendActivationRequestValidator : AbstractValidator<ResendActivationRequest>
{
    public ResendActivationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es requerido.")
            .Must(email => Email.Create(email!).IsSuccess)
            .WithMessage("Debe ingresar un correo electrónico válido.");
    }
}
