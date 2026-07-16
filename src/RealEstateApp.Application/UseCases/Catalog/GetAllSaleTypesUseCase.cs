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

public sealed class GetAllSaleTypesUseCase : IGetAllSaleTypesUseCase
{
    private readonly IGenericRepository<SaleType> _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetAllSaleTypesRequest> _validator;

    public GetAllSaleTypesUseCase(
        IGenericRepository<SaleType> repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<GetAllSaleTypesRequest> validator
    )
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<PagedResult<SaleTypeResponse>>> ExecuteAsync(
        GetAllSaleTypesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<SaleTypeResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<SaleTypeResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Admin))
            && !_currentUser.IsInRole(nameof(Roles.Agent))
        )
            return Result<PagedResult<SaleTypeResponse>>.Failure(
                Error.Forbidden(
                    "Auth.AdminOrAgent",
                    "Solo administradores o agentes pueden listar tipos de venta."
                )
            );

        var options = new QueryOptions<SaleType>
        {
            OrderBy = q => q.OrderBy(st => st.Code),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLowerInvariant();
            options.Filter = st =>
                st.Name.ToLower().Contains(term) || st.Description.ToLower().Contains(term);
        }

        var items = await _repository.GetAllAsync(options, cancellationToken);

        var totalCount = !string.IsNullOrWhiteSpace(request.SearchTerm)
            ? await _repository.CountAsync(
                st =>
                    st.Name.ToLower().Contains(request.SearchTerm.Trim().ToLowerInvariant())
                    || st.Description.ToLower()
                        .Contains(request.SearchTerm.Trim().ToLowerInvariant()),
                cancellationToken
            )
            : await _repository.CountAsync(predicate: null, cancellationToken);

        return Result<PagedResult<SaleTypeResponse>>.Success(
            new PagedResult<SaleTypeResponse>(
                _mapper.Map<IReadOnlyList<SaleTypeResponse>>(items),
                totalCount,
                request.Page,
                request.PageSize
            )
        );
    }
}
