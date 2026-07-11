using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealEstateApp.Application.Dtos.Auth;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Domain.Exceptions;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Infrastructure.Identity.Contexts;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Services;

/// <summary>
/// Servicio de autenticación para la WebApi. Genera tokens JWT.
/// </summary>
public class AccountServiceForWebApi : BaseAccountService, IAccountServiceForWebApi
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly TimeProvider _timeProvider;

    public AccountServiceForWebApi(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IMapper mapper,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AccountServiceForWebApi> logger,
        IdentityContext identityContext,
        TimeProvider timeProvider
    )
        : base(userManager, mapper, logger, identityContext)
    {
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _timeProvider = timeProvider;
    }

    public async Task<LoginResponseDto> AuthenticateAsync(LoginDto login)
    {
        // Buscamos por userName o email
        var user =
            await UserManager.FindByNameAsync(login.UserNameOrEmail)
            ?? await UserManager.FindByEmailAsync(login.UserNameOrEmail);

        if (user is null)
        {
            Logger.LogWarning(
                "Intento de login fallido: usuario {UserNameOrEmail} no encontrado.",
                login.UserNameOrEmail
            );
            throw new DomainException(
                "Auth.InvalidCredentials",
                "Los datos de acceso son inválidos."
            );
        }

        if (!user.Active)
        {
            Logger.LogWarning(
                "Intento de login de usuario inactivo: {UserId}.",
                user.Id
            );
            throw new DomainException(
                "Auth.UserInactive",
                "El usuario se encuentra inactivo y no puede autenticarse."
            );
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            login.Password,
            lockoutOnFailure: true
        );

        if (result.IsLockedOut)
        {
            Logger.LogWarning(
                "Cuenta bloqueada por intentos fallidos: {UserId}.",
                user.Id
            );
            throw new DomainException(
                "Auth.LockedOut",
                "La cuenta se encuentra bloqueada temporalmente debido a múltiples intentos fallidos."
            );
        }

        if (!result.Succeeded)
        {
            Logger.LogWarning(
                "Credenciales inválidas para {UserId}.",
                user.Id
            );
            throw new DomainException(
                "Auth.InvalidCredentials",
                "Los datos de acceso son inválidos."
            );
        }

        var roles = await UserManager.GetRolesAsync(user);
        var (token, expiration) = GenerateJwtToken(user, roles);

        Logger.LogInformation(
            "Inicio de sesión exitoso: {UserId}, roles: {Roles}.",
            user.Id,
            string.Join(", ", roles)
        );

        return new LoginResponseDto
        {
            Token = token,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = roles.ToList().AsReadOnly(),
            Expiration = expiration,
        };
    }

    private (string token, DateTime expiration) GenerateJwtToken(AppUser user, IList<string> roles)
    {
        // Generamos nuestra lista de claims, incluyendo los roles del usuario
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var expiration = _timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_jwtSettings.DurationInMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expiration);
    }
}
