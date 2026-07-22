using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Admin;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Admin;

public sealed class GetAgentByIdUseCase : IGetAgentByIdUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IPropertyRepository _propertyRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAgentByIdRequest> _validator;
    private readonly ILogger<GetAgentByIdUseCase> _logger;

    public GetAgentByIdUseCase(
        UserManager<AppUser> userManager,
        IPropertyRepository propertyRepo,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetAgentByIdRequest> validator,
        ILogger<GetAgentByIdUseCase> logger
    )
    {
        _userManager = userManager;
        _propertyRepo = propertyRepo;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AgentListItemResponse>> ExecuteAsync(
        GetAgentByIdRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<AgentListItemResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<AgentListItemResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Admin))
            && !_currentUser.IsInRole(nameof(Roles.Developer))
        )
            return Result<AgentListItemResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOrDeveloperOnly",
                    "Solo administradores o desarrolladores pueden consultar agentes."
                )
            );

        var user = await _userManager.FindByIdAsync(request.AgentId);
        if (user is null)
            return Result<AgentListItemResponse>.Failure(
                Error.NotFound("Agent.NotFound", "El agente solicitado no existe.")
            );

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(nameof(Roles.Agent)))
            return Result<AgentListItemResponse>.Failure(
                Error.NotFound("Agent.NotFound", "El usuario solicitado no es un agente.")
            );

        var response = _mapper.Map<AgentListItemResponse>(user);
        response = response with
        {
            PropertiesCount = await _propertyRepo.CountAsync(
                p => p.AgentId == user.Id,
                cancellationToken
            ),
        };

        _logger.LogInformation(
            "Admin {AdminId} consultó agente {AgentId}.",
            _currentUser.UserId,
            request.AgentId
        );

        return Result<AgentListItemResponse>.Success(response);
    }
}
