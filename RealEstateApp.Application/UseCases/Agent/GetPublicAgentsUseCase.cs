using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Agent.Responses;
using RealEstateApp.Application.Interfaces.UseCases.Agent;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Agent;

public sealed class GetPublicAgentsUseCase : IGetPublicAgentsUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<GetPublicAgentsRequest> _validator;

    public GetPublicAgentsUseCase(
        IUserRepository userRepository,
        IValidator<GetPublicAgentsRequest> validator
    )
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<Result<PagedResult<PublicAgentResponse>>> ExecuteAsync(
        GetPublicAgentsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<PublicAgentResponse>>();

        var agents = await _userRepository.GetByRoleAsync(
            nameof(Roles.Agent),
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken,
            activeOnly: true,
            userId: request.AgentId,
            searchNamesOnly: true
        );

        var items = agents.Items
            .Select(agent => new PublicAgentResponse(
                agent.Id,
                $"{agent.FirstName} {agent.LastName}".Trim(),
                agent.ProfileImage
            ))
            .ToList();

        return Result<PagedResult<PublicAgentResponse>>.Success(
            new PagedResult<PublicAgentResponse>(items, agents.TotalCount, agents.Page, agents.PageSize)
        );
    }
}
