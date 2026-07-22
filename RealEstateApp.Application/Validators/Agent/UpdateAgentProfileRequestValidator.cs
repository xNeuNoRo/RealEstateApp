using FluentValidation;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Application.Validators.Agent;

public sealed class UpdateAgentProfileRequestValidator
    : AbstractValidator<UpdateAgentProfileRequest>
{
    public UpdateAgentProfileRequestValidator()
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
