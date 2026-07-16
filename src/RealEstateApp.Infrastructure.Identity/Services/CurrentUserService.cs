using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Services;

/// <summary>
/// Implementación de ICurrentUserService que obtiene la información del
/// ClaimsPrincipal vía IHttpContextAccessor y UserManager para el estado Active.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public string? FullName
    {
        get
        {
            var given = User?.FindFirstValue(ClaimTypes.GivenName);
            var surname = User?.FindFirstValue(ClaimTypes.Surname);
            if (given is null && surname is null)
                return null;
            return $"{given} {surname}".Trim();
        }
    }

    public string? ProfilePicturePath
    {
        get
        {
            var userId = UserId;
            if (userId is null)
                return null;

            var user = _userManager
                .Users.AsNoTracking()
                .Select(u => new { u.Id, u.ProfileImage })
                .FirstOrDefault(u => u.Id == userId);

            return user?.ProfileImage;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var claims = User?.FindAll(ClaimTypes.Role) ?? [];
            return claims
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
                .AsReadOnly();
        }
    }

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> IsActiveAsync(CancellationToken cancellationToken = default)
    {
        var userId = UserId;
        if (userId is null)
            return false;

        var active = await _userManager
            .Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Active)
            .FirstOrDefaultAsync(cancellationToken);

        return active;
    }
}
