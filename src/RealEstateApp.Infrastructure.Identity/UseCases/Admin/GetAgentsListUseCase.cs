using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Admin;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Admin;

public sealed class GetAgentsListUseCase : IGetAgentsListUseCase
{
    private readonly IUserRepository _userRepo;
    private readonly IPropertyRepository _propertyRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAgentsListRequest> _validator;
    private readonly ILogger<GetAgentsListUseCase> _logger;

    public GetAgentsListUseCase(
        IUserRepository userRepo,
        IPropertyRepository propertyRepo,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetAgentsListRequest> validator,
        ILogger<GetAgentsListUseCase> logger
    )
    {
        _userRepo = userRepo;
        _propertyRepo = propertyRepo;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AgentListItemResponse>>> ExecuteAsync(
        GetAgentsListRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<AgentListItemResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<AgentListItemResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Admin))
            && !_currentUser.IsInRole(nameof(Roles.Developer))
        )
            return Result<PagedResult<AgentListItemResponse>>.Failure(
                Error.Forbidden(
                    "Auth.AdminOrDeveloperOnly",
                    "Solo administradores o desarrolladores pueden ver el listado de agentes."
                )
            );

        var paged = await _userRepo.GetByRoleAsync(
            nameof(Roles.Agent),
            request.SearchTerm,
            request.Page,
            request.PageSize,
            ct
        );

        var items = new List<AgentListItemResponse>(paged.Items.Count);
        foreach (var user in paged.Items)
        {
            var response = _mapper.Map<AgentListItemResponse>(user);
            response = response with
            {
                PropertiesCount = await _propertyRepo.CountAsync(p => p.AgentId == user.Id, ct),
            };
            items.Add(response);
        }

        _logger.LogInformation(
            "Admin {AdminId} consultó listado de agentes (página {Page}).",
            _currentUser.UserId,
            request.Page
        );

        return Result<PagedResult<AgentListItemResponse>>.Success(
            new PagedResult<AgentListItemResponse>(
                items,
                paged.TotalCount,
                paged.Page,
                paged.PageSize
            )
        );
    }
}
