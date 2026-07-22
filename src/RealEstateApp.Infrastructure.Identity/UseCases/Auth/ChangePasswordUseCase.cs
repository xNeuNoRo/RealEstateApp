using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Domain.Common;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Auth;

public sealed class ChangePasswordUseCase : IChangePasswordUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<ChangePasswordRequest> _validator;
    private readonly ILogger<ChangePasswordUseCase> _logger;

    public ChangePasswordUseCase(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ICurrentUserService currentUser,
        IValidator<ChangePasswordRequest> validator,
        ILogger<ChangePasswordUseCase> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.NotAuthenticated",
                    "Debe iniciar sesión para realizar esta acción."
                )
            );

        var user = await _userManager.FindByIdAsync(_currentUser.UserId);
        if (user is null)
            return Result.Failure(Error.NotFound("Auth.UserNotFound", "Usuario no encontrado."));

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );

        if (!result.Succeeded)
        {
            _logger.LogWarning(
                "Cambio de contraseña fallido para {UserId}: contraseña actual incorrecta.",
                user.Id
            );
            return Result.Failure(
                Error.Validation("Auth.WrongCurrentPassword", "La contraseña actual es incorrecta.")
            );
        }

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("Contraseña cambiada exitosamente para {UserId}.", user.Id);

        return Result.Success();
    }
}
