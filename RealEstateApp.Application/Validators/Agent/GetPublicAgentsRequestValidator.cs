using FluentValidation;
using RealEstateApp.Application.Dtos.Agent.Requests;

namespace RealEstateApp.Application.Validators.Agent;

public sealed class GetPublicAgentsRequestValidator : AbstractValidator<GetPublicAgentsRequest>
{
    public GetPublicAgentsRequestValidator()
    {
        RuleFor(request => request.Page).GreaterThan(0);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 50);
        RuleFor(request => request.SearchTerm).MaximumLength(100);
        RuleFor(request => request.AgentId).MaximumLength(128);
    }
}
