using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Dtos.Auth.Responses;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Domain.Common;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Auth;

public sealed class LoginUseCase : ILoginUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IValidator<LoginRequest> _validator;
    private readonly ILogger<LoginUseCase> _logger;

    public LoginUseCase(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IValidator<LoginRequest> validator,
        ILogger<LoginUseCase> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<AuthResponse>();

        var user =
            await _userManager.FindByNameAsync(request.UserNameOrEmail)
            ?? await _userManager.FindByEmailAsync(request.UserNameOrEmail);

        if (user is null)
        {
            _logger.LogWarning(
                "Login fallido: usuario {Input} no encontrado.",
                request.UserNameOrEmail
            );
            return Result<AuthResponse>.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Los datos de acceso son inválidos.")
            );
        }

        if (!user.Active)
        {
            _logger.LogWarning("Login fallido: usuario {UserId} inactivo.", user.Id);
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(
                    "Auth.UserInactive",
                    "El usuario se encuentra inactivo y no puede iniciar sesión."
                )
            );
        }

        var passwordCheck = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true
        );

        if (passwordCheck.IsLockedOut)
        {
            _logger.LogWarning("Login fallido: cuenta {UserId} bloqueada.", user.Id);
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(
                    "Auth.LockedOut",
                    "La cuenta se encuentra bloqueada temporalmente debido a múltiples intentos fallidos."
                )
            );
        }

        if (!passwordCheck.Succeeded)
        {
            _logger.LogWarning("Login fallido: credenciales inválidas para {UserId}.", user.Id);
            return Result<AuthResponse>.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Los datos de acceso son inválidos.")
            );
        }

        var roles = await _userManager.GetRolesAsync(user);

        string[] validWebRoles = [DefaultRoles.Client, DefaultRoles.Agent, DefaultRoles.Admin];
        if (!roles.Any(validWebRoles.Contains))
        {
            _logger.LogWarning(
                "Login fallido: usuario {UserId} sin rol válido para WebApp.",
                user.Id
            );
            return Result<AuthResponse>.Failure(
                Error.Unauthorized(
                    "Auth.InvalidRole",
                    "El usuario no tiene un rol válido para acceder a la aplicación web."
                )
            );
        }

        await _signInManager.SignInAsync(user, request.RememberMe);

        _logger.LogInformation(
            "Login exitoso: {UserId}, roles: {Roles}.",
            user.Id,
            string.Join(", ", roles)
        );

        var response = new AuthResponse
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.GetDisplayName(),
            Roles = roles.ToList().AsReadOnly(),
            RequiresActivation = false,
        };

        return Result<AuthResponse>.Success(response);
    }
}
