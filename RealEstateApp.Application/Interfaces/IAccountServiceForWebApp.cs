using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Dtos.Auth.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces;

/// <summary>
/// Fachada de autenticación para controladores MVC de la WebApp.
/// Orquesta casos de uso de Auth y mantiene la infraestructura de sesión fuera de MVC.
/// </summary>
public interface IAccountServiceForWebApp
{
    Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<AuthResponse>> RegisterClientAsync(
        RegisterClientRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<AuthResponse>> RegisterAgentAsync(
        RegisterAgentRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result> ActivateAccountAsync(
        ActivateAccountRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result> ResendActivationAsync(
        ResendActivationRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default
    );

    Task LogoutAsync();
}
