using Microsoft.AspNetCore.Identity;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserManager<AppUser> _userManager;

    public UserRepository(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserInfo?> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is null ? null : Map(user);
    }

    public async Task<IReadOnlyList<UserInfo>> GetByIdsAsync(
        IReadOnlyList<string> userIds,
        CancellationToken ct = default)
    {
        var result = new List<UserInfo>(userIds.Count);
        foreach (var id in userIds)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is not null)
                result.Add(Map(user));
        }
        return result;
    }

    private static UserInfo Map(AppUser user) => new()
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
