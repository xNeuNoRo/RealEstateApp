using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class UpdateDeveloperRequestValidator : AbstractValidator<UpdateDeveloperRequest>
{
    public UpdateDeveloperRequestValidator()
    {
        RuleFor(x => x.DeveloperId).NotEmpty().WithMessage("El desarrollador es requerido.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("El nombre es requerido.")
            .MaximumLength(100)
            .WithMessage("El nombre no debe exceder 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("El apellido es requerido.")
            .MaximumLength(100)
            .WithMessage("El apellido no debe exceder 100 caracteres.");

        RuleFor(x => x.IdentityDocument).NotEmpty().WithMessage("La cédula es requerida.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es requerido.")
            .EmailAddress()
            .WithMessage("Debe ingresar un correo electrónico válido.");

        RuleFor(x => x.UserName).NotEmpty().WithMessage("El nombre de usuario es requerido.");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword))
            .WithMessage("La nueva contraseña y la confirmación no coinciden.");
    }
}
