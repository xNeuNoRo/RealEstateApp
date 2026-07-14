using AutoMapper;
using FluentValidation;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using ImprovementEntity = RealEstateApp.Domain.Entities.Improvement;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.UseCases.Improvement;

public sealed class GetAllImprovementsUseCase : IGetAllImprovementsUseCase
{
    private readonly IGenericRepository<ImprovementEntity> _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllImprovementsRequest> _validator;

    public GetAllImprovementsUseCase(
        IGenericRepository<ImprovementEntity> repository,
        IMapper mapper,
        IValidator<GetAllImprovementsRequest> validator
    )
    {
        _repository = repository;
        _mapper = mapper;
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
                i =>
                    i.Name.ToLower().Contains(term)
                    || i.Description.ToLower().Contains(term),
                cancellationToken
            )
            : await _repository.CountAsync(null, cancellationToken);

        var responseItems = _mapper.Map<IReadOnlyList<ImprovementResponse>>(items);

        var pagedResult = new PagedResult<ImprovementResponse>(
            responseItems,
            totalCount,
            request.Page,
            request.PageSize
        );

        return Result<PagedResult<ImprovementResponse>>.Success(pagedResult);
    }
}