using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Auth;

public sealed class ResetPasswordUseCase : IResetPasswordUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IValidator<ResetPasswordRequest> _validator;
    private readonly ILogger<ResetPasswordUseCase> _logger;

    public ResetPasswordUseCase(
        UserManager<AppUser> userManager,
        IValidator<ResetPasswordRequest> validator,
        ILogger<ResetPasswordUseCase> logger
    )
    {
        _userManager = userManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.GetError());

        var email = emailResult.GetValue();

        var user = await _userManager.FindByEmailAsync(email.Value);
        if (user is null)
        {
            _logger.LogWarning(
                "ResetPassword: usuario con email {Email} no encontrado.",
                email.Value
            );
            return Result.Failure(
                Error.NotFound(
                    "Auth.UserNotFound",
                    "No existe un usuario registrado con este correo electrónico."
                )
            );
        }

        var token = Uri.UnescapeDataString(request.Token);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            _logger.LogWarning("ResetPassword fallido para {UserId}: token inválido.", user.Id);
            return Result.Failure(
                Error.Validation(
                    "Auth.InvalidResetToken",
                    "El enlace de restablecimiento no es válido o ya ha expirado."
                )
            );
        }

        _logger.LogInformation("Contraseña restablecida para {UserId}.", user.Id);

        return Result.Success();
    }
}
