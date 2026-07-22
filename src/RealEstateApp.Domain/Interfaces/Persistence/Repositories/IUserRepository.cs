using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Interfaces.Persistence.Repositories;

public interface IUserRepository
{
    Task<UserInfo?> GetByIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserInfo>> GetByIdsAsync(
        IReadOnlyList<string> userIds,
        CancellationToken ct = default
    );
    Task<IReadOnlySet<string>> GetActiveIdsByRoleAsync(
        string roleName,
        CancellationToken ct = default
    );

    Task<PagedResult<UserInfo>> GetByRoleAsync(
        string roleName,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default,
        bool? activeOnly = null,
        string? userId = null,
        bool searchNamesOnly = false
    );

    Task<int> CountByRoleAsync(
        string roleName,
        bool? activeOnly = null,
        CancellationToken ct = default
    );
}
