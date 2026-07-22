using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class ToggleAgentActiveRequestValidator : AbstractValidator<ToggleAgentActiveRequest>
{
    public ToggleAgentActiveRequestValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty().WithMessage("El agente es requerido.");
    }
}
