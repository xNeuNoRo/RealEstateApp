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

public sealed class GetPropertyTypeByIdUseCase : IGetPropertyTypeByIdUseCase
{
    private readonly IGenericRepository<PropertyType> _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetPropertyTypeByIdRequest> _validator;

    public GetPropertyTypeByIdUseCase(
        IGenericRepository<PropertyType> repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<GetPropertyTypeByIdRequest> validator
    )
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<PropertyTypeResponse>> ExecuteAsync(
        GetPropertyTypeByIdRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PropertyTypeResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PropertyTypeResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Admin))
            && !_currentUser.IsInRole(nameof(Roles.Developer))
        )
            return Result<PropertyTypeResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOrDeveloperOnly",
                    "Solo administradores o desarrolladores pueden consultar tipos de propiedad."
                )
            );

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<PropertyTypeResponse>.Failure(
                Error.NotFound(
                    "PropertyType.NotFound",
                    "El tipo de propiedad solicitado no existe."
                )
            );

        return Result<PropertyTypeResponse>.Success(_mapper.Map<PropertyTypeResponse>(entity));
    }
}
