using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Domain.Common;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Auth;

public sealed class ActivateAccountUseCase : IActivateAccountUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IValidator<ActivateAccountRequest> _validator;
    private readonly ILogger<ActivateAccountUseCase> _logger;

    public ActivateAccountUseCase(
        UserManager<AppUser> userManager,
        IValidator<ActivateAccountRequest> validator,
        ILogger<ActivateAccountUseCase> logger
    )
    {
        _userManager = userManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        ActivateAccountRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return Result.Failure(
                Error.NotFound("Auth.UserNotFound", "El usuario especificado no existe.")
            );

        var token = Uri.UnescapeDataString(request.Token);
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            _logger.LogWarning(
                "Activación fallida para {UserId}: token inválido o expirado.",
                user.Id
            );
            return Result.Failure(
                Error.Validation(
                    "Auth.InvalidToken",
                    "El enlace de activación no es válido o ya ha expirado."
                )
            );
        }

        user.Activate();
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Cuenta de {UserId} activada correctamente.", user.Id);

        return Result.Success();
    }
}
