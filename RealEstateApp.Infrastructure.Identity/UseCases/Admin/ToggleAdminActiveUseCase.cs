using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Admin;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Admin;

public sealed class ToggleAdminActiveUseCase : IToggleAdminActiveUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<ToggleAdminActiveRequest> _validator;
    private readonly ILogger<ToggleAdminActiveUseCase> _logger;

    public ToggleAdminActiveUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IValidator<ToggleAdminActiveRequest> validator,
        ILogger<ToggleAdminActiveUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        ToggleAdminActiveRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden gestionar administradores."
                )
            );

        if (request.AdminId == _currentUser.UserId)
            return Result.Failure(
                Error.Validation(
                    "Admin.SelfToggle",
                    "No puede activar o inactivar su propio usuario."
                )
            );

        var user = await _userManager.FindByIdAsync(request.AdminId);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "Administrador no encontrado."));

        if (!await _userManager.IsInRoleAsync(user, nameof(Roles.Admin)))
            return Result.Failure(
                Error.Validation("User.NotAdmin", "El usuario no tiene rol de Administrador.")
            );

        if (user.Active)
            user.Deactivate();
        else
            user.Activate();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Fallo al cambiar estado de admin {AdminId}: {Errors}",
                request.AdminId,
                errors
            );
            return Result.Failure(
                Error.Failure("User.UpdateFailed", "No se pudo actualizar el estado.")
            );
        }

        _logger.LogInformation(
            "Admin {CurrentAdminId} {Action} administrador {TargetAdminId}.",
            _currentUser.UserId,
            user.Active ? "activó" : "inactivó",
            request.AdminId
        );

        return Result.Success();
    }
}
