using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Application.Models.Emails;
using RealEstateApp.Domain.Common;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Auth;

public sealed class ForgotPasswordUseCase : IForgotPasswordUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IValidator<ForgotPasswordRequest> _validator;
    private readonly ILogger<ForgotPasswordUseCase> _logger;

    public ForgotPasswordUseCase(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IValidator<ForgotPasswordRequest> validator,
        ILogger<ForgotPasswordUseCase> logger
    )
    {
        _userManager = userManager;
        _emailService = emailService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.Active)
        {
            _logger.LogInformation(
                "ForgotPassword ignorado para {Email}: usuario no encontrado o inactivo.",
                request.Email
            );
            return Result.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendEmailAsync(
            user.Email!,
            "Restablecimiento de contraseña — RealEstateApp",
            "PasswordReset",
            new PasswordResetModel(
                user.GetDisplayName(),
                $"Para restablecer su contraseña haga clic en: /account/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}"
            ),
            cancellationToken
        );

        _logger.LogInformation("Correo de restablecimiento enviado a {Email}.", user.Email);

        return Result.Success();
    }
}
