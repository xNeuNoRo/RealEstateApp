using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Dtos.Auth;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Domain.Common;
using RealEstateApp.Infrastructure.Identity.Contexts;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;

namespace RealEstateApp.Infrastructure.Identity.Services;

/// <summary>
/// Servicio de autenticación para la WebApp. Usa SignInManager para cookies de ASP.NET Identity.
/// </summary>
public class AccountServiceForWebApp : BaseAccountService, IAccountServiceForWebApp
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;

    public AccountServiceForWebApp(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IMapper mapper,
        ILogger<AccountServiceForWebApp> logger,
        IdentityContext identityContext,
        IEmailService emailService
    )
        : base(userManager, mapper, logger, identityContext)
    {
        _signInManager = signInManager;
        _emailService = emailService;
    }

    public async Task<Result> LoginAsync(LoginDto login)
    {
        var user =
            await UserManager.FindByNameAsync(login.UserNameOrEmail)
            ?? await UserManager.FindByEmailAsync(login.UserNameOrEmail);

        if (user is null)
        {
            Logger.LogWarning(
                "Intento de login fallido WebApp: usuario {UserNameOrEmail} no encontrado.",
                login.UserNameOrEmail
            );
            return Result.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Los datos de acceso son inválidos.")
            );
        }

        if (!user.Active)
        {
            Logger.LogWarning("Intento de login WebApp de usuario inactivo: {UserId}.", user.Id);
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.UserInactive",
                    "El usuario se encuentra inactivo y no puede iniciar sesión."
                )
            );
        }

        var roles = await UserManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            Logger.LogWarning("Usuario {UserId} sin rol válido WebApp.", user.Id);
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.NoRole",
                    "El usuario no tiene un rol válido asignado. Póngase en contacto con un administrador."
                )
            );
        }

        var validWebAppRoles = new[]
        {
            DefaultRoles.Client,
            DefaultRoles.Agent,
            DefaultRoles.Admin,
        };
        if (!roles.Any(r => validWebAppRoles.Contains(r)))
        {
            Logger.LogWarning(
                "Usuario {UserId} con rol no válido para WebApp: {Roles}.",
                user.Id,
                string.Join(", ", roles)
            );
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.InvalidRole",
                    "El usuario no tiene un rol válido asignado. Póngase en contacto con un administrador."
                )
            );
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            login.Password,
            isPersistent: false,
            lockoutOnFailure: true
        );

        if (result.IsLockedOut)
        {
            Logger.LogWarning("Cuenta bloqueada WebApp: {UserId}.", user.Id);
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.LockedOut",
                    "La cuenta se encuentra bloqueada temporalmente debido a múltiples intentos fallidos."
                )
            );
        }

        if (!result.Succeeded)
        {
            Logger.LogWarning("Credenciales inválidas WebApp para {UserId}.", user.Id);
            return Result.Failure(
                Error.Unauthorized("Auth.InvalidCredentials", "Los datos de acceso son inválidos.")
            );
        }

        Logger.LogInformation(
            "Inicio de sesión WebApp exitoso: {UserId}, roles: {Roles}.",
            user.Id,
            string.Join(", ", roles)
        );

        return Result.Success();
    }

    /// <summary>
    /// Registra un cliente con estado Inactivo y envía correo de activación.
    /// </summary>
    public async Task<Result> RegisterClientAsync(RegisterUserDto register)
    {
        if (register.Role != DefaultRoles.Client)
            return Result.Failure(
                Error.Validation("Auth.InvalidRole", "El rol seleccionado no es Cliente.")
            );

        await RegisterUserAsync(register);

        var user = await UserManager.FindByNameAsync(register.UserName);
        if (user is null)
            return Result.Failure(
                Error.Failure("Auth.RegistrationFailed", "No se pudo verificar el registro.")
            );

        user.Active = false;
        user.EmailConfirmed = false;
        await UserManager.UpdateAsync(user);

        var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        try
        {
            await _emailService.SendEmailAsync(
                user.Email!,
                "Activación de cuenta en RealEstateApp",
                "AccountActivation",
                new Application.Models.Emails.AccountActivationModel(
                    UserName: user.GetDisplayName(),
                    ActivationUrl: encodedToken
                ),
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            Logger.LogWarning(
                ex,
                "No se pudo enviar correo de activación a {Email}. Usuario {UserId} creado pero inactivo.",
                user.Email,
                user.Id
            );
        }

        Logger.LogInformation(
            "Cliente {UserName} registrado en WebApp (inactivo, email activación enviado).",
            register.UserName
        );

        return Result.Success();
    }

    /// <summary>
    /// Registra un agente con estado Inactivo. Sin email de activación (admin activa manualmente).
    /// </summary>
    public async Task<Result> RegisterAgentAsync(RegisterUserDto register)
    {
        if (register.Role != DefaultRoles.Agent)
            return Result.Failure(
                Error.Validation("Auth.InvalidRole", "El rol seleccionado no es Agente.")
            );

        await RegisterUserAsync(register);

        var user = await UserManager.FindByNameAsync(register.UserName);
        if (user is null)
            return Result.Failure(
                Error.Failure("Auth.RegistrationFailed", "No se pudo verificar el registro.")
            );

        user.Active = false;
        user.EmailConfirmed = false;
        await UserManager.UpdateAsync(user);

        Logger.LogInformation(
            "Agente {UserName} registrado en WebApp (inactivo, pendiente de activación por admin).",
            register.UserName
        );

        return Result.Success();
    }

    public async Task<Result> ActivateAccountAsync(string userId, string token)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(Error.NotFound("Auth.UserNotFound", "El usuario no existe."));

        var decodedToken = Uri.UnescapeDataString(token);
        var result = await UserManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            Logger.LogWarning(
                "Fallo activación de cuenta {UserId}: {Errors}.",
                userId,
                string.Join(", ", errors)
            );
            return Result.Failure(
                Error.Validation("Auth.ActivationFailed", "No se pudo activar la cuenta.")
            );
        }

        user.Active = true;
        await UserManager.UpdateAsync(user);

        Logger.LogInformation("Cuenta {UserId} activada correctamente.", userId);
        return Result.Success();
    }

    public async Task<Result> ResendActivationAsync(string email)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure(
                Error.NotFound("Auth.UserNotFound", "No existe un usuario con ese correo.")
            );

        if (user.Active)
            return Result.Failure(
                Error.Conflict("Auth.AlreadyActive", "La cuenta ya está activa.")
            );

        var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        try
        {
            await _emailService.SendEmailAsync(
                user.Email!,
                "Activación de cuenta en RealEstateApp",
                "AccountActivation",
                new Application.Models.Emails.AccountActivationModel(
                    UserName: user.GetDisplayName(),
                    ActivationUrl: encodedToken
                ),
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "No se pudo reenviar correo de activación a {Email}.", email);
            return Result.Failure(
                Error.Failure(
                    "Email.SendFailed",
                    "No se pudo enviar el correo de activación. Intente nuevamente."
                )
            );
        }

        Logger.LogInformation("Correo de activación reenviado a {Email}.", email);
        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user is null || !user.Active)
        {
            Logger.LogInformation(
                "Solicitud de reset password para email no válido o inactivo: {Email}.",
                email
            );
            return Result.Success();
        }

        var token = await UserManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        try
        {
            await _emailService.SendEmailAsync(
                user.Email!,
                "Restablecimiento de contraseña - RealEstateApp",
                "PasswordReset",
                new Application.Models.Emails.PasswordResetModel(
                    FullName: user.GetDisplayName(),
                    ResetLink: encodedToken
                ),
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "No se pudo enviar correo de reset password a {Email}.", email);
        }

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure(
                Error.NotFound("Auth.UserNotFound", "No existe un usuario con ese correo.")
            );

        var decodedToken = Uri.UnescapeDataString(token);
        var result = await UserManager.ResetPasswordAsync(user, decodedToken, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            Logger.LogWarning(
                "Fallo reset password {UserId}: {Errors}.",
                user.Id,
                string.Join(", ", errors)
            );
            return Result.Failure(
                Error.Validation(
                    "Auth.ResetPasswordFailed",
                    "No se pudo restablecer la contraseña."
                )
            );
        }

        Logger.LogInformation("Contraseña restablecida para {UserId}.", user.Id);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword
    )
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(Error.NotFound("Auth.UserNotFound", "El usuario no existe."));

        var result = await UserManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            Logger.LogWarning(
                "Fallo cambio contraseña {UserId}: {Errors}.",
                userId,
                string.Join(", ", errors)
            );
            return Result.Failure(
                Error.Validation("Auth.ChangePasswordFailed", "No se pudo cambiar la contraseña.")
            );
        }

        await _signInManager.RefreshSignInAsync(user);

        Logger.LogInformation("Contraseña cambiada para {UserId}.", userId);
        return Result.Success();
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        Logger.LogInformation("Sesión WebApp cerrada.");
    }
}
