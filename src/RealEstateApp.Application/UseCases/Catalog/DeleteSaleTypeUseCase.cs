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
using PropertyEntity = RealEstateApp.Domain.Entities.Property;

namespace RealEstateApp.Application.UseCases.Catalog;

public sealed class DeleteSaleTypeUseCase : IDeleteSaleTypeUseCase
{
    private readonly IGenericRepository<SaleType> _saleTypeRepo;
    private readonly IPropertyRepository _propertyRepo;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<DeleteSaleTypeRequest> _validator;
    private readonly ILogger<DeleteSaleTypeUseCase> _logger;

    public DeleteSaleTypeUseCase(
        IGenericRepository<SaleType> saleTypeRepo,
        IPropertyRepository propertyRepo,
        IFileService fileService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<DeleteSaleTypeRequest> validator,
        ILogger<DeleteSaleTypeUseCase> logger
    )
    {
        _saleTypeRepo = saleTypeRepo;
        _propertyRepo = propertyRepo;
        _fileService = fileService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        DeleteSaleTypeRequest request,
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
                    "Solo administradores pueden eliminar tipos de venta."
                )
            );

        var saleType = await _saleTypeRepo.GetByIdAsync(request.Id, cancellationToken);
        if (saleType is null)
            return Result.Failure(
                Error.NotFound("SaleType.NotFound", "No se encontró el tipo de venta especificado.")
            );

        var properties = await _propertyRepo.GetAllAsync(
            new QueryOptions<PropertyEntity>
            {
                Filter = p => p.SaleTypeId == request.Id,
                Includes = [p => p.Images],
            },
            cancellationToken
        );

        foreach (var property in properties)
        {
            foreach (var img in property.Images)
            {
                try
                {
                    _fileService.DeleteFile(img.Url);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al eliminar imagen {Url}.", img.Url);
                }
            }
        }

        _saleTypeRepo.Delete(saleType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tipo de venta {SaleTypeId} y {PropertyCount} propiedades asociadas eliminadas por admin {AdminId}.",
            request.Id,
            properties.Count,
            _currentUser.UserId
        );

        return Result.Success();
    }
}
