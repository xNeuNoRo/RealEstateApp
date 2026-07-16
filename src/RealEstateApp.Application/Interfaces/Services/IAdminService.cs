using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.Services;

public interface IAdminService
{
    Task<Result<AdminDashboardResponse>> GetDashboardAsync(CancellationToken ct = default);
    Task<Result<PagedResult<AgentListItemResponse>>> GetAgentsAsync(
        GetAgentsListRequest request,
        CancellationToken ct = default
    );
    Task<Result> ToggleAgentStatusAsync(string agentId, CancellationToken ct = default);
    Task<Result> DeleteAgentAsync(string agentId, CancellationToken ct = default);
    Task<Result<PagedResult<AdminListItemResponse>>> GetAdminsAsync(
        GetAdminsListRequest request,
        CancellationToken ct = default
    );
    Task<Result<AdminResponse>> CreateAdminAsync(
        CreateAdminRequest request,
        CancellationToken ct = default
    );
    Task<Result<AdminResponse>> UpdateAdminAsync(
        UpdateAdminRequest request,
        CancellationToken ct = default
    );
    Task<Result> ToggleAdminStatusAsync(string adminId, CancellationToken ct = default);
    Task<Result<PagedResult<DeveloperListItemResponse>>> GetDevelopersAsync(
        GetDevelopersListRequest request,
        CancellationToken ct = default
    );
    Task<Result<DeveloperResponse>> CreateDeveloperAsync(
        CreateDeveloperRequest request,
        CancellationToken ct = default
    );
    Task<Result<DeveloperResponse>> UpdateDeveloperAsync(
        UpdateDeveloperRequest request,
        CancellationToken ct = default
    );
    Task<Result> ToggleDeveloperStatusAsync(string devId, CancellationToken ct = default);
    Task<Result<PagedResult<PropertyTypeResponse>>> GetPropertyTypesAsync(
        GetAllPropertyTypesRequest request,
        CancellationToken ct = default
    );
    Task<Result<PropertyTypeResponse>> CreatePropertyTypeAsync(
        CreatePropertyTypeRequest request,
        CancellationToken ct = default
    );
    Task<Result<PropertyTypeResponse>> UpdatePropertyTypeAsync(
        UpdatePropertyTypeRequest request,
        CancellationToken ct = default
    );
    Task<Result> DeletePropertyTypeAsync(int id, CancellationToken ct = default);
    Task<Result<PagedResult<SaleTypeResponse>>> GetSaleTypesAsync(
        GetAllSaleTypesRequest request,
        CancellationToken ct = default
    );
    Task<Result<SaleTypeResponse>> CreateSaleTypeAsync(
        CreateSaleTypeRequest request,
        CancellationToken ct = default
    );
    Task<Result> UpdateSaleTypeAsync(UpdateSaleTypeRequest request, CancellationToken ct = default);
    Task<Result> DeleteSaleTypeAsync(int id, CancellationToken ct = default);
    Task<Result<PagedResult<ImprovementResponse>>> GetImprovementsAsync(
        GetAllImprovementsRequest request,
        CancellationToken ct = default
    );
    Task<Result<ImprovementResponse>> CreateImprovementAsync(
        CreateImprovementRequest request,
        CancellationToken ct = default
    );
    Task<Result> UpdateImprovementAsync(
        UpdateImprovementRequest request,
        CancellationToken ct = default
    );
    Task<Result> DeleteImprovementAsync(int id, CancellationToken ct = default);
}
