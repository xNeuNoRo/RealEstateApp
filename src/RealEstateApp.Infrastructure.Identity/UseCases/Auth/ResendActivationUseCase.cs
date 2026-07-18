using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Application.Models.Emails;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Auth;

public sealed class ResendActivationUseCase : IResendActivationUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IValidator<ResendActivationRequest> _validator;
    private readonly ILogger<ResendActivationUseCase> _logger;

    public ResendActivationUseCase(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IValidator<ResendActivationRequest> validator,
        ILogger<ResendActivationUseCase> logger
    )
    {
        _userManager = userManager;
        _emailService = emailService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        ResendActivationRequest request,
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
        if (user is null || user.Active || user.EmailConfirmed)
        {
            _logger.LogInformation(
                "Reenvío de activación ignorado para {Email}: usuario no encontrado o ya activo.",
                email.Value
            );
            return Result.Success();
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendEmailAsync(
            user.Email!,
            "Activación de cuenta en RealEstateApp",
            "AccountActivation",
            new AccountActivationModel(
                user.GetDisplayName(),
                $"Por favor confirme su cuenta haciendo clic en el siguiente enlace: /account/activate?userId={user.Id}&token={Uri.EscapeDataString(token)}"
            ),
            cancellationToken
        );

        _logger.LogInformation("Correo de activación reenviado a {Email}.", user.Email);

        return Result.Success();
    }
}
