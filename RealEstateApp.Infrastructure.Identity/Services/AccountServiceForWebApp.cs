using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Dtos.Auth.Responses;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.UseCases.Auth;
using RealEstateApp.Domain.Common;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Services;

/// <summary>
/// Fachada de Auth para MVC. Mantiene el controlador desacoplado de la composición
/// interna de casos de uso y concentra el cierre de sesión de Identity.
/// </summary>
public sealed class AccountServiceForWebApp : IAccountServiceForWebApp
{
    private readonly ILoginUseCase _loginUseCase;
    private readonly IRegisterClientUseCase _registerClientUseCase;
    private readonly IRegisterAgentUseCase _registerAgentUseCase;
    private readonly IActivateAccountUseCase _activateAccountUseCase;
    private readonly IResendActivationUseCase _resendActivationUseCase;
    private readonly IForgotPasswordUseCase _forgotPasswordUseCase;
    private readonly IResetPasswordUseCase _resetPasswordUseCase;
    private readonly IChangePasswordUseCase _changePasswordUseCase;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AccountServiceForWebApp> _logger;

    public AccountServiceForWebApp(
        ILoginUseCase loginUseCase,
        IRegisterClientUseCase registerClientUseCase,
        IRegisterAgentUseCase registerAgentUseCase,
        IActivateAccountUseCase activateAccountUseCase,
        IResendActivationUseCase resendActivationUseCase,
        IForgotPasswordUseCase forgotPasswordUseCase,
        IResetPasswordUseCase resetPasswordUseCase,
        IChangePasswordUseCase changePasswordUseCase,
        SignInManager<AppUser> signInManager,
        ILogger<AccountServiceForWebApp> logger
    )
    {
        _loginUseCase = loginUseCase;
        _registerClientUseCase = registerClientUseCase;
        _registerAgentUseCase = registerAgentUseCase;
        _activateAccountUseCase = activateAccountUseCase;
        _resendActivationUseCase = resendActivationUseCase;
        _forgotPasswordUseCase = forgotPasswordUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
        _changePasswordUseCase = changePasswordUseCase;
        _signInManager = signInManager;
        _logger = logger;
    }

    public Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    ) => _loginUseCase.ExecuteAsync(request, cancellationToken);

    public Task<Result<AuthResponse>> RegisterClientAsync(
        RegisterClientRequest request,
        CancellationToken cancellationToken = default
    ) => _registerClientUseCase.ExecuteAsync(request, cancellationToken);

    public Task<Result<AuthResponse>> RegisterAgentAsync(
        RegisterAgentRequest request,
        CancellationToken cancellationToken = default
    ) => _registerAgentUseCase.ExecuteAsync(request, cancellationToken);

    public Task<Result> ActivateAccountAsync(
        ActivateAccountRequest request,
        CancellationToken cancellationToken = default
    ) => _activateAccountUseCase.ExecuteAsync(request, cancellationToken);

    public Task<Result> ResendActivationAsync(
        ResendActivationRequest request,
        CancellationToken cancellationToken = default
    ) => _resendActivationUseCase.ExecuteAsync(request, cancellationToken);

    public Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default
    ) => _forgotPasswordUseCase.ExecuteAsync(request, cancellationToken);

    public Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default
    ) => _resetPasswordUseCase.ExecuteAsync(request, cancellationToken);

    public Task<Result> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default
    ) => _changePasswordUseCase.ExecuteAsync(request, cancellationToken);

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Sesión WebApp cerrada.");
    }
}
