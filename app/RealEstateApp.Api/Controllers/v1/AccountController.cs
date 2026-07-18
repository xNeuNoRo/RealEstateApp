using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Api.Controllers.Base;
using RealEstateApp.Application.Dtos.Auth;
using RealEstateApp.Application.Interfaces;

namespace RealEstateApp.Api.Controllers.v1;

[ApiVersion("1.0")]
[Authorize(Policy = Policies.ApiAuthorizationPolicies.ApiAccess)]
public class AccountController : BaseApiController
{
    private readonly IAccountServiceForWebApi _accountService;

    public AccountController(IAccountServiceForWebApi accountService)
    {
        _accountService = accountService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _accountService.AuthenticateAsync(dto);
        return Success(result);
    }

    [HttpPost("register/developer")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterDeveloper([FromBody] RegisterUserDto dto)
    {
        dto.Role = "Developer";
        var result = await _accountService.RegisterUserAsync(dto);
        return Success(result);
    }

    [HttpPost("register/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserDto dto)
    {
        dto.Role = "Admin";
        var result = await _accountService.RegisterUserAsync(dto);
        return Success(result);
    }
}
