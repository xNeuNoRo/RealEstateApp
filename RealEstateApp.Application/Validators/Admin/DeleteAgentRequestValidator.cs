using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class DeleteAgentRequestValidator : AbstractValidator<DeleteAgentRequest>
{
    public DeleteAgentRequestValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty().WithMessage("El agente es requerido.");
    }
}
