using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Catalog;

public sealed class UpdateSaleTypeUseCase : IUpdateSaleTypeUseCase
{
    private readonly IGenericRepository<SaleType> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<UpdateSaleTypeRequest> _validator;

    public UpdateSaleTypeUseCase(
        IGenericRepository<SaleType> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<UpdateSaleTypeRequest> validator
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(
        UpdateSaleTypeRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden gestionar tipos de venta."
                )
            );

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(
                Error.NotFound("SaleType.NotFound", "No se encontró el tipo de venta especificado.")
            );

        var result = entity.Update(request.Code, request.Name, request.Description);
        if (result.IsFailure)
            return Result.Failure(result.GetError());

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
