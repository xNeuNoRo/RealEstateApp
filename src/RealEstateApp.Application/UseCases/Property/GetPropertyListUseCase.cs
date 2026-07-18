using System.Linq.Expressions;
using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using PropertyEntity = RealEstateApp.Domain.Entities.Property;

namespace RealEstateApp.Application.UseCases.Property;

public sealed class GetPropertyListUseCase : IGetPropertyListUseCase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetPropertyListRequest> _validator;

    public GetPropertyListUseCase(
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IValidator<GetPropertyListRequest> validator
    )
    {
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PagedResult<PropertyListItemResponse>>> ExecuteAsync(
        GetPropertyListRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<PropertyListItemResponse>>();

        var filter = BuildFilter(request);

        var options = new QueryOptions<PropertyEntity>
        {
            Filter = filter,
            Includes = [p => p.PropertyType!, p => p.SaleType!, p => p.Images],
            OrderBy = q => q.OrderByDescending(p => p.CreatedAt),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        var properties = await _propertyRepository.GetAllAsync(options, cancellationToken);
        var totalCount = await _propertyRepository.CountAsync(filter, cancellationToken);

        var items = _mapper.Map<List<PropertyListItemResponse>>(properties);
        var agentIds = properties.Select(p => p.AgentId).Distinct().ToList();
        var agents = await _userRepository.GetByIdsAsync(agentIds, cancellationToken);
        var agentMap = agents.ToDictionary(a => a.Id);

        foreach (var item in items)
        {
            var prop = properties.First(p => p.Id == item.Id);
            if (agentMap.TryGetValue(prop.AgentId, out var agent))
                item.AgentName = $"{agent.FirstName} {agent.LastName}".Trim();
        }

        return Result<PagedResult<PropertyListItemResponse>>.Success(
            new PagedResult<PropertyListItemResponse>(
                items,
                totalCount,
                request.Page,
                request.PageSize
            )
        );
    }

    private static Expression<Func<PropertyEntity, bool>> BuildFilter(GetPropertyListRequest req)
    {
        return p =>
            (req.IncludeAllStatuses || p.Status == PropertyStatus.Available)
            && (
                string.IsNullOrWhiteSpace(req.SearchTerm)
                || (p.Description.Contains(req.SearchTerm) || p.Code.Value.Contains(req.SearchTerm))
            )
            && (!req.PriceMin.HasValue || p.Price.Amount >= req.PriceMin.Value)
            && (!req.PriceMax.HasValue || p.Price.Amount <= req.PriceMax.Value)
            && (!req.SizeMin.HasValue || p.Size.Area >= req.SizeMin.Value)
            && (!req.SizeMax.HasValue || p.Size.Area <= req.SizeMax.Value)
            && (!req.Bedrooms.HasValue || p.Bedrooms == req.Bedrooms.Value)
            && (!req.Bathrooms.HasValue || p.Bathrooms == req.Bathrooms.Value)
            && (!req.PropertyTypeId.HasValue || p.PropertyTypeId == req.PropertyTypeId.Value)
            && (!req.SaleTypeId.HasValue || p.SaleTypeId == req.SaleTypeId.Value)
            && (string.IsNullOrWhiteSpace(req.Code) || p.Code.Value.Contains(req.Code))
            && (string.IsNullOrWhiteSpace(req.AgentId) || p.AgentId == req.AgentId);
    }
}
