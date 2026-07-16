using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Catalog;

public sealed class GetAllPropertyTypesUseCase : IGetAllPropertyTypesUseCase
{
    private readonly IGenericRepository<PropertyType> _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetAllPropertyTypesRequest> _validator;

    public GetAllPropertyTypesUseCase(
        IGenericRepository<PropertyType> repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<GetAllPropertyTypesRequest> validator
    )
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<PagedResult<PropertyTypeResponse>>> ExecuteAsync(
        GetAllPropertyTypesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<PropertyTypeResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<PropertyTypeResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Admin))
            && !_currentUser.IsInRole(nameof(Roles.Agent))
        )
            return Result<PagedResult<PropertyTypeResponse>>.Failure(
                Error.Forbidden(
                    "Auth.AdminOrAgent",
                    "Solo administradores o agentes pueden listar tipos de propiedad."
                )
            );

        string? term = null;
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            term = request.SearchTerm.Trim().ToLowerInvariant();
        }

        var options = new QueryOptions<PropertyType>
        {
            OrderBy = q => q.OrderBy(pt => pt.Name),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        if (term is not null)
        {
            options.Filter = pt =>
                pt.Name.ToLower().Contains(term) || pt.Description.ToLower().Contains(term);
        }

        var items = await _repository.GetAllAsync(options, cancellationToken);

        var totalCount = term is not null
            ? await _repository.CountAsync(
                pt => pt.Name.ToLower().Contains(term) || pt.Description.ToLower().Contains(term),
                cancellationToken
            )
            : await _repository.CountAsync(null, cancellationToken);

        var responseItems = _mapper.Map<IReadOnlyList<PropertyTypeResponse>>(items);

        var pagedResult = new PagedResult<PropertyTypeResponse>(
            responseItems,
            totalCount,
            request.Page,
            request.PageSize
        );

        return Result<PagedResult<PropertyTypeResponse>>.Success(pagedResult);
    }
}
