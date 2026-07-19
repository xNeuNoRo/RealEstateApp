using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Identity.Contexts;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IdentityContext _identityContext;

    public UserRepository(UserManager<AppUser> userManager, IdentityContext identityContext)
    {
        _userManager = userManager;
        _identityContext = identityContext;
    }

    public async Task<UserInfo?> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : Map(user);
    }

    public async Task<IReadOnlyList<UserInfo>> GetByIdsAsync(
        IReadOnlyList<string> userIds,
        CancellationToken ct = default
    )
    {
        if (userIds.Count == 0)
            return Array.Empty<UserInfo>();

        var distinctIds = userIds.Distinct().ToList();
        var users = await _userManager.Users
            .Where(user => distinctIds.Contains(user.Id))
            .ToListAsync(ct);

        return users.Select(Map).ToList();
    }

    public async Task<PagedResult<UserInfo>> GetByRoleAsync(
        string roleName,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default,
        bool? activeOnly = null,
        string? userId = null,
        bool searchNamesOnly = false
    )
    {
        var role = await _identityContext
            .Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == roleName, ct);

        if (role is null)
            return new PagedResult<UserInfo>(Array.Empty<UserInfo>(), 0, page, pageSize);

        var userIdsInRole = _identityContext
            .UserRoles.Where(ur => ur.RoleId == role.Id)
            .Select(ur => ur.UserId);

        var query = _userManager.Users.Where(u => userIdsInRole.Contains(u.Id));

        if (activeOnly.HasValue)
            query = query.Where(user => user.Active == activeOnly.Value);

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(user => user.Id == userId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            query = searchNamesOnly
                ? query.Where(u =>
                    u.FirstName.ToLower().Contains(term) || u.LastName.ToLower().Contains(term)
                )
                : query.Where(u =>
                    u.FirstName.ToLower().Contains(term)
                    || u.LastName.ToLower().Contains(term)
                    || (u.Email != null && u.Email.ToLower().Contains(term))
                    || (u.UserName != null && u.UserName.ToLower().Contains(term))
                );
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = users.Select(Map).ToList();
        return new PagedResult<UserInfo>(items, totalCount, page, pageSize);
    }

    public async Task<int> CountByRoleAsync(
        string roleName,
        bool? activeOnly = null,
        CancellationToken ct = default
    )
    {
        var role = await _identityContext
            .Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == roleName, ct);

        if (role is null)
            return 0;

        var userIdsInRole = _identityContext
            .UserRoles.Where(ur => ur.RoleId == role.Id)
            .Select(ur => ur.UserId);

        var query = _userManager.Users.Where(u => userIdsInRole.Contains(u.Id));

        if (activeOnly.HasValue)
            query = query.Where(u => u.Active == activeOnly.Value);

        return await query.CountAsync(ct);
    }

    private static UserInfo Map(AppUser user) =>
        new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            ProfileImage = user.ProfileImage,
            UserName = user.UserName,
        };
}
