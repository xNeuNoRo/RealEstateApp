using FluentValidation;
using RealEstateApp.Application.Dtos.Agent.Requests;

namespace RealEstateApp.Application.Validators.Agent;

public sealed class UpdateAgentProfileRequestValidator
    : AbstractValidator<UpdateAgentProfileRequest>
{
    public UpdateAgentProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
    }
}
