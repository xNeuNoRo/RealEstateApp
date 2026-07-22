using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class GetAgentByIdRequestValidator : AbstractValidator<GetAgentByIdRequest>
{
    public GetAgentByIdRequestValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty().WithMessage("El ID del agente es requerido.");
    }
}
