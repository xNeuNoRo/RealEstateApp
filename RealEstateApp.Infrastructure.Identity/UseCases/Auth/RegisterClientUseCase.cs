using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Dtos.Auth.Responses;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Application.Models.Emails;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Auth;

public sealed class RegisterClientUseCase : IRegisterClientUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IFileService _fileService;
    private readonly IValidator<RegisterClientRequest> _validator;
    private readonly ILogger<RegisterClientUseCase> _logger;

    public RegisterClientUseCase(
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IFileService fileService,
        IValidator<RegisterClientRequest> validator,
        ILogger<RegisterClientUseCase> logger
    )
    {
        _userManager = userManager;
        _emailService = emailService;
        _fileService = fileService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        RegisterClientRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<AuthResponse>();

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result<AuthResponse>.Failure(emailResult.GetError());

        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure)
            return Result<AuthResponse>.Failure(phoneResult.GetError());

        var email = emailResult.GetValue();
        var phone = phoneResult.GetValue();

        if (await _userManager.FindByNameAsync(request.UserName) is not null)
            return Result<AuthResponse>.Failure(
                Error.Conflict(
                    "Auth.UserNameTaken",
                    "Ya existe un usuario registrado con este nombre de usuario."
                )
            );

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Result<AuthResponse>.Failure(
                Error.Conflict(
                    "Auth.EmailTaken",
                    "Ya existe un usuario registrado con este correo electrónico."
                )
            );

        var user = new AppUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserName = request.UserName,
            Email = email.Value,
            Active = false,
            EmailConfirmed = false,
        };
        user.SetPhone(phone.Value);

        if (request.PhotoFile is not null)
        {
            if (!_fileService.IsImageValid(request.PhotoFile))
                return Result<AuthResponse>.Failure(
                    Error.Validation(
                        "Auth.InvalidImage",
                        $"La imagen no tiene un formato válido (JPEG, PNG o WebP máximo {FileConstants.MaxImageFileSizeBytes / (1024 * 1024)} MB)."
                    )
                );

            user.ProfileImage = await _fileService.UploadFileAsync(request.PhotoFile, "users");
        }

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Registro de cliente fallido para {User}: {Errors}",
                request.UserName,
                errors
            );
            return Result<AuthResponse>.Failure(
                Error.Failure(
                    "Auth.RegistrationFailed",
                    "No fue posible completar el registro. Intente nuevamente más tarde."
                )
            );
        }

        var roleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Client);
        if (!roleResult.Succeeded)
        {
            _logger.LogError(
                "No se pudo asignar rol Client a {UserId}: {Errors}",
                user.Id,
                string.Join(", ", roleResult.Errors.Select(e => e.Description))
            );
            return Result<AuthResponse>.Failure(
                Error.Failure("Auth.RoleAssignmentFailed", "No se pudo completar el registro.")
            );
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendEmailAsync(
            user.Email!,
            "Activación de cuenta en RealEstateApp",
            "AccountActivation",
            new AccountActivationModel(
                user.GetDisplayName(),
                $"{request.Origin.TrimEnd('/')}/Auth/ActivateAccount?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}"
            ),
            cancellationToken
        );

        _logger.LogInformation("Cliente {UserId} registrado. Activación pendiente.", user.Id);

        return Result<AuthResponse>.Success(
            new AuthResponse
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.GetDisplayName(),
                RequiresActivation = true,
                Message =
                    "Su cuenta ha sido creada correctamente. Revise su correo electrónico para activar su usuario.",
            }
        );
    }
}
