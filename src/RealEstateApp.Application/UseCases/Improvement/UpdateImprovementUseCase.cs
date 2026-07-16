using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using ImprovementEntity = RealEstateApp.Domain.Entities.Improvement;

namespace RealEstateApp.Application.UseCases.Improvement;

public sealed class UpdateImprovementUseCase : IUpdateImprovementUseCase
{
    private readonly IGenericRepository<ImprovementEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<UpdateImprovementRequest> _validator;

    public UpdateImprovementUseCase(
        IGenericRepository<ImprovementEntity> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<UpdateImprovementRequest> validator
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result> ExecuteAsync(
        UpdateImprovementRequest request,
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
                Error.Forbidden("Auth.AdminOnly", "Solo administradores pueden gestionar mejoras.")
            );

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Failure(
                Error.NotFound("Improvement.NotFound", "No se encontró la mejora especificada.")
            );

        // Validar unicidad del nombre (excluyendo la entidad actual)
        var nameExists = await _repository.ExistsAsync(
            i => i.Name == request.Name && i.Id != request.Id,
            cancellationToken
        );
        if (nameExists)
            return Result.Failure(
                Error.Validation(
                    "Improvement.NameDuplicate",
                    "Ya existe una mejora con ese nombre."
                )
            );

        var updateResult = entity.Update(request.Name, request.Description);
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.GetError());

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
