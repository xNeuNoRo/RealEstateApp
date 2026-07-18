using FluentValidation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Application.Validators.Auth;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es requerido.")
            .Must(email => Email.Create(email!).IsSuccess)
            .WithMessage("Debe ingresar un correo electrónico válido.");

        RuleFor(x => x.Token).NotEmpty().WithMessage("El token de restablecimiento es requerido.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("La nueva contraseña es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("La confirmación de contraseña es requerida.")
            .Equal(x => x.NewPassword)
            .WithMessage("La contraseña y la confirmación de contraseña no coinciden.");
    }
}
