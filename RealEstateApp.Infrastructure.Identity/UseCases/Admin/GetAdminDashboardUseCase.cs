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

public sealed class GetAdminDashboardUseCase : IGetAdminDashboardUseCase
{
    private readonly IPropertyRepository _propertyRepo;
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetAdminDashboardRequest> _validator;
    private readonly ILogger<GetAdminDashboardUseCase> _logger;

    public GetAdminDashboardUseCase(
        IPropertyRepository propertyRepo,
        IUserRepository userRepo,
        ICurrentUserService currentUser,
        IValidator<GetAdminDashboardRequest> validator,
        ILogger<GetAdminDashboardUseCase> logger
    )
    {
        _propertyRepo = propertyRepo;
        _userRepo = userRepo;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AdminDashboardResponse>> ExecuteAsync(
        GetAdminDashboardRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<AdminDashboardResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<AdminDashboardResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result<AdminDashboardResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden acceder al dashboard."
                )
            );

        var availableCount = await _propertyRepo.CountAsync(
            p => p.Status == PropertyStatus.Available,
            ct
        );
        var soldCount = await _propertyRepo.CountAsync(p => p.Status == PropertyStatus.Sold, ct);

        var activeAgents = await _userRepo.CountByRoleAsync(
            nameof(Roles.Agent),
            activeOnly: true,
            ct
        );
        var inactiveAgents = await _userRepo.CountByRoleAsync(
            nameof(Roles.Agent),
            activeOnly: false,
            ct
        );

        var activeClients = await _userRepo.CountByRoleAsync(
            nameof(Roles.Client),
            activeOnly: true,
            ct
        );
        var inactiveClients = await _userRepo.CountByRoleAsync(
            nameof(Roles.Client),
            activeOnly: false,
            ct
        );

        var activeDevs = await _userRepo.CountByRoleAsync(
            nameof(Roles.Developer),
            activeOnly: true,
            ct
        );
        var inactiveDevs = await _userRepo.CountByRoleAsync(
            nameof(Roles.Developer),
            activeOnly: false,
            ct
        );

        _logger.LogInformation("Dashboard consultado por admin {AdminId}.", _currentUser.UserId);

        return Result<AdminDashboardResponse>.Success(
            new AdminDashboardResponse(
                availableCount,
                soldCount,
                activeAgents,
                inactiveAgents,
                activeClients,
                inactiveClients,
                activeDevs,
                inactiveDevs
            )
        );
    }
}
