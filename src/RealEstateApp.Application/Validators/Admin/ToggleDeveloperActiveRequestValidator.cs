using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class ToggleDeveloperActiveRequestValidator
    : AbstractValidator<ToggleDeveloperActiveRequest>
{
    public ToggleDeveloperActiveRequestValidator()
    {
        RuleFor(x => x.DeveloperId).NotEmpty().WithMessage("El desarrollador es requerido.");
    }
}
