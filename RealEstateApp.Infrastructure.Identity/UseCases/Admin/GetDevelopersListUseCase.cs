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

public sealed class GetDevelopersListUseCase : IGetDevelopersListUseCase
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetDevelopersListRequest> _validator;
    private readonly ILogger<GetDevelopersListUseCase> _logger;

    public GetDevelopersListUseCase(
        IUserRepository userRepo,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetDevelopersListRequest> validator,
        ILogger<GetDevelopersListUseCase> logger
    )
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<PagedResult<DeveloperListItemResponse>>> ExecuteAsync(
        GetDevelopersListRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<DeveloperListItemResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<DeveloperListItemResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result<PagedResult<DeveloperListItemResponse>>.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden gestionar desarrolladores."
                )
            );

        var paged = await _userRepo.GetByRoleAsync(
            nameof(Roles.Developer),
            request.SearchTerm,
            request.Page,
            request.PageSize,
            ct
        );

        var items = _mapper.Map<IReadOnlyList<DeveloperListItemResponse>>(paged.Items);

        _logger.LogInformation(
            "Admin {AdminId} consultó listado de desarrolladores (página {Page}).",
            _currentUser.UserId,
            request.Page
        );

        return Result<PagedResult<DeveloperListItemResponse>>.Success(
            new PagedResult<DeveloperListItemResponse>(
                items,
                paged.TotalCount,
                paged.Page,
                paged.PageSize
            )
        );
    }
}
