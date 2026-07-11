using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Interfaces.Persistence.Repositories;

public interface IUserRepository
{
    Task<UserInfo?> GetByIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserInfo>> GetByIdsAsync(IReadOnlyList<string> userIds, CancellationToken ct = default);
}
