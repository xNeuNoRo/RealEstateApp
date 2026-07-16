using RealEstateApp.Application.Dtos.Auth;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces;

/// <summary>
/// Servicio de autenticación para la WebApp (cookie-based, no JWT).
/// </summary>
public interface IAccountServiceForWebApp
{
    Task<Result> LoginAsync(LoginDto login);
    Task<Result> RegisterClientAsync(RegisterUserDto register);
    Task<Result> RegisterAgentAsync(RegisterUserDto register);
    Task<Result> ActivateAccountAsync(string userId, string token);
    Task<Result> ResendActivationAsync(string email);
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(string email, string token, string newPassword);
    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task LogoutAsync();
}
