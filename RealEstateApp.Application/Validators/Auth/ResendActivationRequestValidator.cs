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

        RuleFor(x => x.Origin)
            .Must(IsHttpOrigin)
            .WithMessage("El origen de la aplicación no es válido.");
    }

    private static bool IsHttpOrigin(string origin) =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
