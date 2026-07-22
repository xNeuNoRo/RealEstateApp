using FluentValidation;
using RealEstateApp.Application.Dtos.Client.Requests;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Application.Validators.Client;

public sealed class UpdateClientProfileRequestValidator
    : AbstractValidator<UpdateClientProfileRequest>
{
    public UpdateClientProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("El teléfono es requerido.")
            .Must(phone => PhoneNumber.Create(phone!).IsSuccess)
            .WithMessage("Debe ingresar un número telefónico válido.")
            .MaximumLength(20);
    }
}
