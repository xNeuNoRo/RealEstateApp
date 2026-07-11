using System.Linq.Expressions;
using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using PropertyEntity = RealEstateApp.Domain.Entities.Property;

namespace RealEstateApp.Application.UseCases.Property;

public sealed class GetAgentPropertiesUseCase : IGetAgentPropertiesUseCase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAgentPropertiesRequest> _validator;

    public GetAgentPropertiesUseCase(
        IPropertyRepository propertyRepository,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetAgentPropertiesRequest> validator)
    {
        _propertyRepository = propertyRepository;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PagedResult<PropertySummaryResponse>>> ExecuteAsync(
        GetAgentPropertiesRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<PropertySummaryResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<PropertySummaryResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión para realizar esta acción."));

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<PagedResult<PropertySummaryResponse>>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden ver sus propiedades."));

        var options = new QueryOptions<PropertyEntity>
        {
            Includes =
            [
                p => p.Images,
            ],
            OrderBy = q => q.OrderByDescending(p => p.CreatedAt),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        var properties = await _propertyRepository.GetByAgentAsync(
            _currentUser.UserId,
            request.Status,
            options,
            cancellationToken);

        Expression<Func<PropertyEntity, bool>> countFilter = request.Status.HasValue
            ? p => p.AgentId == _currentUser.UserId && p.Status == request.Status.Value
            : p => p.AgentId == _currentUser.UserId;

        var totalCount = await _propertyRepository.CountAsync(countFilter, cancellationToken);
        var items = _mapper.Map<List<PropertySummaryResponse>>(properties);

        return Result<PagedResult<PropertySummaryResponse>>.Success(
            new PagedResult<PropertySummaryResponse>(items, totalCount, request.Page, request.PageSize));
    }
}
