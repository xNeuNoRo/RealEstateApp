using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using ImprovementEntity = RealEstateApp.Domain.Entities.Improvement;

namespace RealEstateApp.Application.UseCases.Improvement;

public sealed class GetAllImprovementsUseCase : IGetAllImprovementsUseCase
{
    private readonly IGenericRepository<ImprovementEntity> _repository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetAllImprovementsRequest> _validator;

    public GetAllImprovementsUseCase(
        IGenericRepository<ImprovementEntity> repository,
        IPropertyRepository propertyRepository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<GetAllImprovementsRequest> validator
    )
    {
        _repository = repository;
        _propertyRepository = propertyRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<PagedResult<ImprovementResponse>>> ExecuteAsync(
        GetAllImprovementsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<ImprovementResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<ImprovementResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Admin))
            && !_currentUser.IsInRole(nameof(Roles.Agent))
        )
            return Result<PagedResult<ImprovementResponse>>.Failure(
                Error.Forbidden(
                    "Auth.AdminOrAgent",
                    "Solo administradores o agentes pueden listar mejoras."
                )
            );

        string? term = null;
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            term = request.SearchTerm.Trim().ToLowerInvariant();
        }

        var options = new QueryOptions<ImprovementEntity>
        {
            OrderBy = q => q.OrderBy(i => i.Name),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        if (term is not null)
        {
            options.Filter = i =>
                i.Name.ToLower().Contains(term) || i.Description.ToLower().Contains(term);
        }

        var items = await _repository.GetAllAsync(options, cancellationToken);

        var totalCount = term is not null
            ? await _repository.CountAsync(
                i => i.Name.ToLower().Contains(term) || i.Description.ToLower().Contains(term),
                cancellationToken
            )
            : await _repository.CountAsync(null, cancellationToken);

        var responseItems = _mapper.Map<IReadOnlyList<ImprovementResponse>>(items);

        var enrichedItems = new List<ImprovementResponse>(responseItems.Count);
        foreach (var item in responseItems)
        {
            var count = await _propertyRepository.CountAsync(
                p => p.Improvements.Any(pi => pi.ImprovementId == item.Id),
                cancellationToken
            );
            enrichedItems.Add(item with { PropertiesCount = count });
        }

        var pagedResult = new PagedResult<ImprovementResponse>(
            enrichedItems.AsReadOnly(),
            totalCount,
            request.Page,
            request.PageSize
        );

        return Result<PagedResult<ImprovementResponse>>.Success(pagedResult);
    }
}
