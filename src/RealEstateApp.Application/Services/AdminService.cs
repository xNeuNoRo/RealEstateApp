using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Admin;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Services;

public sealed class AdminService : IAdminService
{
    private readonly IGetAdminDashboardUseCase _dashboardUC;
    private readonly IGetAgentsListUseCase _getAgentsUC;
    private readonly IToggleAgentActiveUseCase _toggleAgentUC;
    private readonly IDeleteAgentUseCase _deleteAgentUC;
    private readonly IGetAdminsListUseCase _getAdminsUC;
    private readonly ICreateAdminUseCase _createAdminUC;
    private readonly IUpdateAdminUseCase _updateAdminUC;
    private readonly IToggleAdminActiveUseCase _toggleAdminUC;
    private readonly IGetDevelopersListUseCase _getDevelopersUC;
    private readonly ICreateDeveloperUseCase _createDevUC;
    private readonly IUpdateDeveloperUseCase _updateDevUC;
    private readonly IToggleDeveloperActiveUseCase _toggleDevUC;
    private readonly ICreatePropertyTypeUseCase _createPropTypeUC;
    private readonly IUpdatePropertyTypeUseCase _updatePropTypeUC;
    private readonly IDeletePropertyTypeUseCase _deletePropTypeUC;
    private readonly IGetAllPropertyTypesUseCase _getPropTypesUC;
    private readonly ICreateSaleTypeUseCase _createSaleTypeUC;
    private readonly IUpdateSaleTypeUseCase _updateSaleTypeUC;
    private readonly IDeleteSaleTypeUseCase _deleteSaleTypeUC;
    private readonly IGetAllSaleTypesUseCase _getSaleTypesUC;
    private readonly ICreateImprovementUseCase _createImpUC;
    private readonly IUpdateImprovementUseCase _updateImpUC;
    private readonly IDeleteImprovementUseCase _deleteImpUC;
    private readonly IGetAllImprovementsUseCase _getImpsUC;
    private readonly ICurrentUserService _currentUser;

    public AdminService(
        IGetAdminDashboardUseCase dashboardUC,
        IGetAgentsListUseCase getAgentsUC,
        IToggleAgentActiveUseCase toggleAgentUC,
        IDeleteAgentUseCase deleteAgentUC,
        IGetAdminsListUseCase getAdminsUC,
        ICreateAdminUseCase createAdminUC,
        IUpdateAdminUseCase updateAdminUC,
        IToggleAdminActiveUseCase toggleAdminUC,
        IGetDevelopersListUseCase getDevelopersUC,
        ICreateDeveloperUseCase createDevUC,
        IUpdateDeveloperUseCase updateDevUC,
        IToggleDeveloperActiveUseCase toggleDevUC,
        ICreatePropertyTypeUseCase createPropTypeUC,
        IUpdatePropertyTypeUseCase updatePropTypeUC,
        IDeletePropertyTypeUseCase deletePropTypeUC,
        IGetAllPropertyTypesUseCase getPropTypesUC,
        ICreateSaleTypeUseCase createSaleTypeUC,
        IUpdateSaleTypeUseCase updateSaleTypeUC,
        IDeleteSaleTypeUseCase deleteSaleTypeUC,
        IGetAllSaleTypesUseCase getSaleTypesUC,
        ICreateImprovementUseCase createImpUC,
        IUpdateImprovementUseCase updateImpUC,
        IDeleteImprovementUseCase deleteImpUC,
        IGetAllImprovementsUseCase getImpsUC,
        ICurrentUserService currentUser
    )
    {
        _dashboardUC = dashboardUC;
        _getAgentsUC = getAgentsUC;
        _toggleAgentUC = toggleAgentUC;
        _deleteAgentUC = deleteAgentUC;
        _getAdminsUC = getAdminsUC;
        _createAdminUC = createAdminUC;
        _updateAdminUC = updateAdminUC;
        _toggleAdminUC = toggleAdminUC;
        _getDevelopersUC = getDevelopersUC;
        _createDevUC = createDevUC;
        _updateDevUC = updateDevUC;
        _toggleDevUC = toggleDevUC;
        _createPropTypeUC = createPropTypeUC;
        _updatePropTypeUC = updatePropTypeUC;
        _deletePropTypeUC = deletePropTypeUC;
        _getPropTypesUC = getPropTypesUC;
        _createSaleTypeUC = createSaleTypeUC;
        _updateSaleTypeUC = updateSaleTypeUC;
        _deleteSaleTypeUC = deleteSaleTypeUC;
        _getSaleTypesUC = getSaleTypesUC;
        _createImpUC = createImpUC;
        _updateImpUC = updateImpUC;
        _deleteImpUC = deleteImpUC;
        _getImpsUC = getImpsUC;
        _currentUser = currentUser;
    }

    private Result<T> Forbidden<T>(string detail = "Acceso denegado.") =>
        Result<T>.Failure(Error.Forbidden("Auth.Forbidden", detail));

    private Result Forbidden(string detail = "Acceso denegado.") =>
        Result.Failure(Error.Forbidden("Auth.Forbidden", detail));

    private bool RequireAdmin() => _currentUser.IsInRole(nameof(Roles.Admin));

    public async Task<Result<AdminDashboardResponse>> GetDashboardAsync(
        CancellationToken ct = default
    )
    {
        return await _dashboardUC.ExecuteAsync(new GetAdminDashboardRequest(), ct);
    }

    public async Task<Result<PagedResult<AgentListItemResponse>>> GetAgentsAsync(
        GetAgentsListRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<PagedResult<AgentListItemResponse>>(
                "Solo administradores pueden ver el listado de agentes."
            );

        return await _getAgentsUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> ToggleAgentStatusAsync(string agentId, CancellationToken ct = default)
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden cambiar el estado de un agente.");

        return await _toggleAgentUC.ExecuteAsync(new ToggleAgentActiveRequest(agentId), ct);
    }

    public async Task<Result> DeleteAgentAsync(string agentId, CancellationToken ct = default)
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden eliminar agentes.");

        return await _deleteAgentUC.ExecuteAsync(new DeleteAgentRequest(agentId), ct);
    }

    public async Task<Result<PagedResult<AdminListItemResponse>>> GetAdminsAsync(
        GetAdminsListRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<PagedResult<AdminListItemResponse>>(
                "Solo administradores pueden ver el listado de administradores."
            );

        return await _getAdminsUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<AdminResponse>> CreateAdminAsync(
        CreateAdminRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<AdminResponse>("Solo administradores pueden crear administradores.");

        return await _createAdminUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<AdminResponse>> UpdateAdminAsync(
        UpdateAdminRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<AdminResponse>(
                "Solo administradores pueden modificar administradores."
            );

        return await _updateAdminUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> ToggleAdminStatusAsync(string adminId, CancellationToken ct = default)
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden cambiar el estado de un administrador.");

        return await _toggleAdminUC.ExecuteAsync(new ToggleAdminActiveRequest(adminId), ct);
    }

    public async Task<Result<PagedResult<DeveloperListItemResponse>>> GetDevelopersAsync(
        GetDevelopersListRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<PagedResult<DeveloperListItemResponse>>(
                "Solo administradores pueden ver desarrolladores."
            );

        return await _getDevelopersUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<DeveloperResponse>> CreateDeveloperAsync(
        CreateDeveloperRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<DeveloperResponse>(
                "Solo administradores pueden crear desarrolladores."
            );

        return await _createDevUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<DeveloperResponse>> UpdateDeveloperAsync(
        UpdateDeveloperRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<DeveloperResponse>(
                "Solo administradores pueden modificar desarrolladores."
            );

        return await _updateDevUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> ToggleDeveloperStatusAsync(
        string devId,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden cambiar el estado de un desarrollador.");

        return await _toggleDevUC.ExecuteAsync(new ToggleDeveloperActiveRequest(devId), ct);
    }

    public async Task<Result<PagedResult<PropertyTypeResponse>>> GetPropertyTypesAsync(
        GetAllPropertyTypesRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<PagedResult<PropertyTypeResponse>>(
                "Solo administradores pueden ver tipos de propiedad."
            );

        return await _getPropTypesUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<PropertyTypeResponse>> CreatePropertyTypeAsync(
        CreatePropertyTypeRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<PropertyTypeResponse>(
                "Solo administradores pueden crear tipos de propiedad."
            );

        return await _createPropTypeUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<PropertyTypeResponse>> UpdatePropertyTypeAsync(
        UpdatePropertyTypeRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<PropertyTypeResponse>(
                "Solo administradores pueden modificar tipos de propiedad."
            );

        return await _updatePropTypeUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> DeletePropertyTypeAsync(int id, CancellationToken ct = default)
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden eliminar tipos de propiedad.");

        return await _deletePropTypeUC.ExecuteAsync(new DeletePropertyTypeRequest(id), ct);
    }

    public async Task<Result<PagedResult<SaleTypeResponse>>> GetSaleTypesAsync(
        GetAllSaleTypesRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<PagedResult<SaleTypeResponse>>(
                "Solo administradores pueden ver tipos de venta."
            );

        return await _getSaleTypesUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<SaleTypeResponse>> CreateSaleTypeAsync(
        CreateSaleTypeRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<SaleTypeResponse>("Solo administradores pueden crear tipos de venta.");

        return await _createSaleTypeUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> UpdateSaleTypeAsync(
        UpdateSaleTypeRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden modificar tipos de venta.");

        return await _updateSaleTypeUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> DeleteSaleTypeAsync(int id, CancellationToken ct = default)
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden eliminar tipos de venta.");

        return await _deleteSaleTypeUC.ExecuteAsync(new DeleteSaleTypeRequest(id), ct);
    }

    public async Task<Result<PagedResult<ImprovementResponse>>> GetImprovementsAsync(
        GetAllImprovementsRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<PagedResult<ImprovementResponse>>(
                "Solo administradores pueden ver mejoras."
            );

        return await _getImpsUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<ImprovementResponse>> CreateImprovementAsync(
        CreateImprovementRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden<ImprovementResponse>("Solo administradores pueden crear mejoras.");

        return await _createImpUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> UpdateImprovementAsync(
        UpdateImprovementRequest request,
        CancellationToken ct = default
    )
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden modificar mejoras.");

        return await _updateImpUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> DeleteImprovementAsync(int id, CancellationToken ct = default)
    {
        if (!RequireAdmin())
            return Forbidden("Solo administradores pueden eliminar mejoras.");

        return await _deleteImpUC.ExecuteAsync(new DeleteImprovementRequest(id), ct);
    }
}
