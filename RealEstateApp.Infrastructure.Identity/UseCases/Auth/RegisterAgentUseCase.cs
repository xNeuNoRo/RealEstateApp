using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Dtos.Auth.Responses;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Auth;

public sealed class RegisterAgentUseCase : IRegisterAgentUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IFileService _fileService;
    private readonly IValidator<RegisterAgentRequest> _validator;
    private readonly ILogger<RegisterAgentUseCase> _logger;

    public RegisterAgentUseCase(
        UserManager<AppUser> userManager,
        IFileService fileService,
        IValidator<RegisterAgentRequest> validator,
        ILogger<RegisterAgentUseCase> logger
    )
    {
        _userManager = userManager;
        _fileService = fileService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        RegisterAgentRequest request,
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
                "Registro de agente fallido para {User}: {Errors}",
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

        var roleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Agent);
        if (!roleResult.Succeeded)
        {
            _logger.LogError(
                "No se pudo asignar rol Agent a {UserId}: {Errors}",
                user.Id,
                string.Join(", ", roleResult.Errors.Select(e => e.Description))
            );
            return Result<AuthResponse>.Failure(
                Error.Failure("Auth.RoleAssignmentFailed", "No se pudo completar el registro.")
            );
        }

        _logger.LogInformation(
            "Agente {UserId} registrado. Pendiente de activación por administrador.",
            user.Id
        );

        return Result<AuthResponse>.Success(
            new AuthResponse
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.GetDisplayName(),
                RequiresActivation = true,
                Message =
                    "Su cuenta de agente ha sido creada correctamente. Un administrador debe activar su usuario antes de que pueda iniciar sesión.",
            }
        );
    }
}
