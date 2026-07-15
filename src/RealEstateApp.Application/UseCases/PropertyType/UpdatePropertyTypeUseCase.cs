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

public sealed class UpdatePropertyTypeUseCase : IUpdatePropertyTypeUseCase
{
    private readonly IGenericRepository<PropertyType> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<UpdatePropertyTypeRequest> _validator;
    private readonly AutoMapper.IMapper _mapper;

    public UpdatePropertyTypeUseCase(
        IGenericRepository<PropertyType> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<UpdatePropertyTypeRequest> validator,
        AutoMapper.IMapper mapper
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<Result<PropertyTypeResponse>> ExecuteAsync(
        UpdatePropertyTypeRequest request,
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

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<PropertyTypeResponse>.Failure(
                Error.NotFound("PropertyType.NotFound", "No se encontró el tipo de propiedad especificado.")
            );

        var nameExists = await _repository.ExistsAsync(
            pt => pt.Name == request.Name && pt.Id != request.Id,
            cancellationToken
        );
        if (nameExists)
            return Result<PropertyTypeResponse>.Failure(
                Error.Validation("PropertyType.NameDuplicate", "Ya existe un tipo de propiedad con ese nombre.")
            );

        var updateResult = entity.Update(request.Name, request.Description);
        if (updateResult.IsFailure)
            return Result<PropertyTypeResponse>.Failure(updateResult.GetError());

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PropertyTypeResponse>.Success(_mapper.Map<PropertyTypeResponse>(entity));
    }
}