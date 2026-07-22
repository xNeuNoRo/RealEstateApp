using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class CreateAdminRequestValidator : AbstractValidator<CreateAdminRequest>
{
    public CreateAdminRequestValidator()
    {
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

        RuleFor(x => x.IdentityDocument)
            .NotEmpty()
            .WithMessage("La cédula es requerida.")
            .Must(cedula => IdentityDocument.Create(cedula!).IsSuccess)
            .WithMessage("La cédula no es válida.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es requerido.")
            .Must(email => Email.Create(email!).IsSuccess)
            .WithMessage("Debe ingresar un correo electrónico válido.");

        RuleFor(x => x.UserName).NotEmpty().WithMessage("El nombre de usuario es requerido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("La confirmación de contraseña es requerida.")
            .Equal(x => x.Password)
            .WithMessage("La contraseña y la confirmación no coinciden.");
    }
}
