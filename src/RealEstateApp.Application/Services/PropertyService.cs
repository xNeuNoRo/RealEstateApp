using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Services;

public sealed class PropertyService : IPropertyService
{
    private readonly ICreatePropertyUseCase _createUC;
    private readonly IUpdatePropertyUseCase _updateUC;
    private readonly IDeletePropertyUseCase _deleteUC;
    private readonly IGetPropertyListUseCase _listUC;
    private readonly IGetPropertyDetailUseCase _detailUC;
    private readonly ISearchPropertyByCodeUseCase _searchUC;
    private readonly IGetAgentPropertiesUseCase _agentPropsUC;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<PropertyService> _logger;

    public PropertyService(
        ICreatePropertyUseCase createUC,
        IUpdatePropertyUseCase updateUC,
        IDeletePropertyUseCase deleteUC,
        IGetPropertyListUseCase listUC,
        IGetPropertyDetailUseCase detailUC,
        ISearchPropertyByCodeUseCase searchUC,
        IGetAgentPropertiesUseCase agentPropsUC,
        ICurrentUserService currentUser,
        ILogger<PropertyService> logger
    )
    {
        _createUC = createUC;
        _updateUC = updateUC;
        _deleteUC = deleteUC;
        _listUC = listUC;
        _detailUC = detailUC;
        _searchUC = searchUC;
        _agentPropsUC = agentPropsUC;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<CreatePropertyResponse>> CreateAsync(
        CreatePropertyRequest request,
        CancellationToken ct = default
    )
    {
        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<CreatePropertyResponse>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden publicar propiedades.")
            );

        return await _createUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> UpdateAsync(
        UpdatePropertyRequest request,
        CancellationToken ct = default
    )
    {
        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden modificar propiedades.")
            );

        return await _updateUC.ExecuteAsync(request, ct);
    }

    public async Task<Result> DeleteAsync(int propertyId, CancellationToken ct = default)
    {
        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden eliminar propiedades.")
            );

        return await _deleteUC.ExecuteAsync(new DeletePropertyRequest(propertyId), ct);
    }

    public async Task<Result<PagedResult<PropertyListItemResponse>>> GetListAsync(
        GetPropertyListRequest request,
        CancellationToken ct = default
    )
    {
        return await _listUC.ExecuteAsync(request, ct);
    }

    public async Task<Result<PropertyDetailResponse>> GetDetailAsync(
        int propertyId,
        CancellationToken ct = default
    )
    {
        return await _detailUC.ExecuteAsync(new GetPropertyDetailRequest(propertyId), ct);
    }

    public async Task<Result<PropertyDetailResponse>> SearchByCodeAsync(
        string code,
        CancellationToken ct = default
    )
    {
        return await _searchUC.ExecuteAsync(new SearchPropertyByCodeRequest(code), ct);
    }

    public async Task<Result<PagedResult<PropertySummaryResponse>>> GetAgentPropertiesAsync(
        GetAgentPropertiesRequest request,
        CancellationToken ct = default
    )
    {
        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<PagedResult<PropertySummaryResponse>>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden ver sus propiedades.")
            );

        return await _agentPropsUC.ExecuteAsync(request, ct);
    }
}
