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

public sealed class ToggleAgentActiveUseCase : IToggleAgentActiveUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<ToggleAgentActiveRequest> _validator;
    private readonly ILogger<ToggleAgentActiveUseCase> _logger;

    public ToggleAgentActiveUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IValidator<ToggleAgentActiveRequest> validator,
        ILogger<ToggleAgentActiveUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        ToggleAgentActiveRequest request,
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
                Error.Forbidden("Auth.AdminOnly", "Solo administradores pueden gestionar agentes.")
            );

        var user = await _userManager.FindByIdAsync(request.AgentId);
        if (user is null)
            return Result.Failure(Error.NotFound("User.NotFound", "Agente no encontrado."));

        if (!await _userManager.IsInRoleAsync(user, nameof(Roles.Agent)))
            return Result.Failure(
                Error.Validation("User.NotAgent", "El usuario no tiene rol de Agente.")
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
                "Fallo al cambiar estado de agente {AgentId}: {Errors}",
                request.AgentId,
                errors
            );
            return Result.Failure(
                Error.Failure("User.UpdateFailed", "No se pudo actualizar el estado del agente.")
            );
        }

        _logger.LogInformation(
            "Admin {AdminId} {Action} agente {AgentId}.",
            _currentUser.UserId,
            user.Active ? "activó" : "inactivó",
            request.AgentId
        );

        return Result.Success();
    }
}
