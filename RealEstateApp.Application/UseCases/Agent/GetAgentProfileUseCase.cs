using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Agent.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Agent;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Agent;

public sealed class GetAgentProfileUseCase : IGetAgentProfileUseCase
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAgentProfileRequest> _validator;

    public GetAgentProfileUseCase(
        IUserRepository userRepo,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetAgentProfileRequest> validator
    )
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<AgentProfileResponse>> ExecuteAsync(
        GetAgentProfileRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<AgentProfileResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<AgentProfileResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<AgentProfileResponse>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden consultar su perfil.")
            );

        var userInfo = await _userRepo.GetByIdAsync(_currentUser.UserId, ct);
        if (userInfo is null)
            return Result<AgentProfileResponse>.Failure(
                Error.NotFound("User.NotFound", "No se encontró su información de perfil.")
            );

        var response = _mapper.Map<AgentProfileResponse>(userInfo);
        return Result<AgentProfileResponse>.Success(response);
    }
}
