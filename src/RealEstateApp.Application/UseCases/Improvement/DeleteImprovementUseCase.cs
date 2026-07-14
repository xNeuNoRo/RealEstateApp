using FluentValidation;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using ImprovementEntity = RealEstateApp.Domain.Entities.Improvement;

namespace RealEstateApp.Application.UseCases.Catalog;

public sealed class DeleteImprovementUseCase : IDeleteImprovementUseCase
{
    private readonly IGenericRepository<ImprovementEntity> _improvementRepo;
    private readonly IPropertyRepository _propertyRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<DeleteImprovementRequest> _validator;
    private readonly ILogger<DeleteImprovementUseCase> _logger;

    public DeleteImprovementUseCase(
        IGenericRepository<ImprovementEntity> improvementRepo,
        IPropertyRepository propertyRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<DeleteImprovementRequest> validator,
        ILogger<DeleteImprovementUseCase> logger
    )
    {
        _improvementRepo = improvementRepo;
        _propertyRepo = propertyRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        DeleteImprovementRequest request,
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
                    "Solo administradores pueden eliminar mejoras."
                )
            );

        var improvement = await _improvementRepo.GetByIdAsync(request.Id, cancellationToken);
        if (improvement is null)
            return Result.Failure(
                Error.NotFound("Improvement.NotFound", "No se encontró la mejora especificada.")
            );

        var inUse = await _propertyRepo.ExistsAsync(
            p => p.Improvements.Any(pi => pi.ImprovementId == request.Id),
            cancellationToken
        );
        if (inUse)
            return Result.Failure(
                Error.Conflict(
                    "Improvement.InUse",
                    "No se puede eliminar: está asociada a una o más propiedades."
                )
            );

        _improvementRepo.Delete(improvement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} eliminó mejora {ImprovementId}.",
            _currentUser.UserId,
            request.Id
        );

        return Result.Success();
    }
}