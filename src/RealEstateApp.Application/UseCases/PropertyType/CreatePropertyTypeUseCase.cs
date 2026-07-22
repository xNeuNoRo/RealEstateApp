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
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Catalog;

public sealed class CreatePropertyTypeUseCase : ICreatePropertyTypeUseCase
{
    private readonly IGenericRepository<PropertyType> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<CreatePropertyTypeRequest> _validator;

    public CreatePropertyTypeUseCase(
        IGenericRepository<PropertyType> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<CreatePropertyTypeRequest> validator
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PropertyTypeResponse>> ExecuteAsync(
        CreatePropertyTypeRequest request,
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

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result<PropertyTypeResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden gestionar tipos de propiedad."
                )
            );

        var result = PropertyType.Create(request.Name, request.Description);
        if (result.IsFailure)
            return Result<PropertyTypeResponse>.Failure(result.GetError());

        var entity = result.GetValue();

        var nameExists = await _repository.ExistsAsync(
            pt => pt.Name == request.Name, cancellationToken);
        if (nameExists)
            return Result<PropertyTypeResponse>.Failure(
                Error.Validation("PropertyType.NameDuplicate",
                    "Ya existe un tipo de propiedad con ese nombre."));

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PropertyTypeResponse>.Success(_mapper.Map<PropertyTypeResponse>(entity));
    }
}
