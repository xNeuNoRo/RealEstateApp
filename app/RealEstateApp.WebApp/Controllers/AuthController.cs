using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RealEstateApp.Application.Adapters;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Dtos.Auth.Responses;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Auth;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.WebApp.Extensions;

namespace RealEstateApp.WebApp.Controllers;

[AllowAnonymous]
public sealed class AuthController : BaseController
{
    private readonly IAccountServiceForWebApp _accountService;
    private readonly string _appUrl;

    public AuthController(
        ICurrentUserService currentUser,
        IAccountServiceForWebApp accountService,
        IConfiguration configuration
    )
        : base(currentUser)
    {
        _accountService = accountService;
        _appUrl = GetAppUrl(configuration);
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (CurrentUser.IsAuthenticated)
        {
            if (await CurrentUser.IsActiveAsync(HttpContext.RequestAborted))
                return RedirectByRole(CurrentUser.Roles);

            await _accountService.LogoutAsync();
            this.SetErrorMessage(
                "Tu cuenta se encuentra inactiva. Contacta al administrador o completa su activación."
            );
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth-login")]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null,
        CancellationToken cancellationToken = default
    )
    {
        if (CurrentUser.IsAuthenticated)
        {
            if (await CurrentUser.IsActiveAsync(cancellationToken))
                return RedirectByRole(CurrentUser.Roles);

            await _accountService.LogoutAsync();
        }

        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var result = await _accountService.LoginAsync(
            new LoginRequest(model.UserNameOrEmail, model.Password, model.RememberMe),
            cancellationToken
        );

        if (result.IsFailure)
            return ViewWithError(model, result.GetError());

        var user = result.GetValue();
        this.SetSuccessMessage($"Bienvenido de vuelta, {user.FullName}.");

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectByRole(user.Roles);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (CurrentUser.IsAuthenticated)
            return RedirectByRole(CurrentUser.Roles);

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(16 * 1024 * 1024)]
    [EnableRateLimiting("auth-register")]
    public async Task<IActionResult> Register(
        RegisterViewModel model,
        IFormFile? photoFile,
        CancellationToken cancellationToken = default
    )
    {
        if (CurrentUser.IsAuthenticated)
            return RedirectByRole(CurrentUser.Roles);

        if (!string.IsNullOrWhiteSpace(model.Website))
            return RegistrationCompleted(model.Email, model.SelectedRole);

        if (photoFile is null || photoFile.Length == 0)
            ModelState.AddModelError("PhotoFile", "La foto de usuario es requerida.");

        if (!ModelState.IsValid)
            return View(model);

        Result<AuthResponse> result;

        if (model.SelectedRole == nameof(Roles.Client))
        {
            result = await _accountService.RegisterClientAsync(
                new RegisterClientRequest(
                    model.FirstName,
                    model.LastName,
                    model.Phone,
                    photoFile.ToAppFile(),
                    model.UserName,
                    model.Email,
                    model.Password,
                    model.ConfirmPassword,
                    _appUrl
                ),
                cancellationToken
            );
        }
        else
        {
            result = await _accountService.RegisterAgentAsync(
                new RegisterAgentRequest(
                    model.FirstName,
                    model.LastName,
                    model.Phone,
                    photoFile.ToAppFile(),
                    model.UserName,
                    model.Email,
                    model.Password,
                    model.ConfirmPassword
                ),
                cancellationToken
            );
        }

        if (result.IsFailure)
        {
            var error = result.GetError();
            var field = error.Code switch
            {
                "Auth.EmailTaken" => nameof(model.Email),
                "Auth.UserNameTaken" => nameof(model.UserName),
                "Auth.InvalidImage" => "PhotoFile",
                _ => string.Empty,
            };
            ModelState.AddModelError(field, error.Message);
            return View(model);
        }

        return RegistrationCompleted(result.GetValue().Email, model.SelectedRole);
    }

    [HttpGet]
    public IActionResult RegistrationConfirmation()
    {
        if (TempData.Peek("RegisteredRole") is null)
            return RedirectToAction(nameof(Register));

        return View();
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth-email")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
            return View(model);

        if (string.IsNullOrWhiteSpace(model.Website))
        {
            var result = await _accountService.ForgotPasswordAsync(
                new ForgotPasswordRequest(model.Email, _appUrl),
                cancellationToken
            );

            if (result.IsFailure)
                return ViewWithError(model, result.GetError());
        }

        this.SetSweetAlert(
            "Si el correo pertenece a una cuenta activa, recibirás instrucciones para restablecer tu contraseña.",
            "info"
        );
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            this.SetWarningMessage("El enlace de restablecimiento no es válido o está incompleto.");
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth-email")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _accountService.ResetPasswordAsync(
            new ResetPasswordRequest(
                model.Email,
                model.Token,
                model.NewPassword,
                model.ConfirmPassword
            ),
            cancellationToken
        );

        if (result.IsFailure)
            return ViewWithError(model, result.GetError());

        this.SetSuccessMessage("Tu contraseña fue restablecida. Ya puedes iniciar sesión.");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResendActivation() => View(new ResendActivationViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth-email")]
    public async Task<IActionResult> ResendActivation(
        ResendActivationViewModel model,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
            return View(model);

        if (string.IsNullOrWhiteSpace(model.Website))
        {
            var result = await _accountService.ResendActivationAsync(
                new ResendActivationRequest(model.Email, _appUrl),
                cancellationToken
            );

            if (result.IsFailure)
                return ViewWithError(model, result.GetError());
        }

        this.SetSweetAlert(
            "Si la cuenta existe y aún está pendiente, recibirás un nuevo enlace de activación.",
            "info"
        );
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> ActivateAccount(
        string? userId,
        string? token,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return View(
                new ActivateAccountViewModel
                {
                    Success = false,
                    Message = "El enlace de activación no es válido o está incompleto.",
                }
            );
        }

        var result = await _accountService.ActivateAccountAsync(
            new ActivateAccountRequest(userId, token),
            cancellationToken
        );

        return View(
            new ActivateAccountViewModel
            {
                UserId = userId,
                Token = token,
                Success = result.IsSuccess,
                Message = result.IsSuccess
                    ? "Tu cuenta fue activada correctamente. Ya puedes iniciar sesión."
                    : result.GetError().Message,
            }
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();
        HttpContext.Session.Clear();
        this.SetSweetAlert("Has cerrado sesión correctamente.", "info");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View("~/Views/Shared/AccessDenied.cshtml");

    private IActionResult RegistrationCompleted(string email, string role)
    {
        TempData["RegisteredEmail"] = email;
        TempData["RegisteredRole"] = role;
        return RedirectToAction(nameof(RegistrationConfirmation));
    }

    private IActionResult RedirectByRole(IEnumerable<string> roles)
    {
        var roleSet = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roleSet.Contains(nameof(Roles.Admin)))
            return RedirectToAction("Index", "Dashboard");
        if (roleSet.Contains(nameof(Roles.Agent)))
            return RedirectToAction("MyProperties", "Property");

        return RedirectToAction("Index", "Property");
    }

    private ViewResult ViewWithError<TModel>(TModel model, Error error)
    {
        ModelState.AddModelError(string.Empty, error.Message);
        return View(model);
    }

    private static string GetAppUrl(IConfiguration configuration)
    {
        var value = configuration["AppUrl"]?.TrimEnd('/');
        if (
            string.IsNullOrWhiteSpace(value)
            || !Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        )
        {
            throw new InvalidOperationException(
                "AppUrl debe contener la URL pública absoluta de la WebApp."
            );
        }

        return value;
    }
}
