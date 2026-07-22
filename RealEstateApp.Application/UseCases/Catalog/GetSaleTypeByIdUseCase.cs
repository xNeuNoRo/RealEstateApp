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

public sealed class GetSaleTypeByIdUseCase : IGetSaleTypeByIdUseCase
{
    private readonly IGenericRepository<SaleType> _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetSaleTypeByIdRequest> _validator;

    public GetSaleTypeByIdUseCase(
        IGenericRepository<SaleType> repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<GetSaleTypeByIdRequest> validator
    )
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<SaleTypeResponse>> ExecuteAsync(
        GetSaleTypeByIdRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<SaleTypeResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<SaleTypeResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Admin))
            && !_currentUser.IsInRole(nameof(Roles.Developer))
        )
            return Result<SaleTypeResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOrDeveloperOnly",
                    "Solo administradores o desarrolladores pueden consultar tipos de venta."
                )
            );

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<SaleTypeResponse>.Failure(
                Error.NotFound("SaleType.NotFound", "El tipo de venta solicitado no existe.")
            );

        return Result<SaleTypeResponse>.Success(_mapper.Map<SaleTypeResponse>(entity));
    }
}
