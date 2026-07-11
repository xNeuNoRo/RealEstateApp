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

namespace RealEstateApp.Application.UseCases.Catalog;

public sealed class DeleteSaleTypeUseCase : IDeleteSaleTypeUseCase
{
    private readonly IGenericRepository<SaleType> _saleTypeRepo;
    private readonly IPropertyRepository _propertyRepo;
    private readonly IGenericRepository<PropertyImage> _imageRepo;
    private readonly IGenericRepository<Message> _messageRepo;
    private readonly IGenericRepository<Offer> _offerRepo;
    private readonly IGenericRepository<FavoriteProperty> _favoriteRepo;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<DeleteSaleTypeRequest> _validator;
    private readonly ILogger<DeleteSaleTypeUseCase> _logger;

    public DeleteSaleTypeUseCase(
        IGenericRepository<SaleType> saleTypeRepo,
        IPropertyRepository propertyRepo,
        IGenericRepository<PropertyImage> imageRepo,
        IGenericRepository<Message> messageRepo,
        IGenericRepository<Offer> offerRepo,
        IGenericRepository<FavoriteProperty> favoriteRepo,
        IFileService fileService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<DeleteSaleTypeRequest> validator,
        ILogger<DeleteSaleTypeUseCase> logger
    )
    {
        _saleTypeRepo = saleTypeRepo;
        _propertyRepo = propertyRepo;
        _imageRepo = imageRepo;
        _messageRepo = messageRepo;
        _offerRepo = offerRepo;
        _favoriteRepo = favoriteRepo;
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
            new QueryOptions<Domain.Entities.Property>
            {
                Filter = p => p.SaleTypeId == request.Id,
                Includes = [p => p.Images],
            },
            cancellationToken
        );

        foreach (var property in properties)
        {
            DeletePropertyCascade(property);
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

    private void DeletePropertyCascade(Domain.Entities.Property property)
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

        var images = _imageRepo.Query().Where(i => i.PropertyId == property.Id).ToList();
        foreach (var img in images)
            _imageRepo.Delete(img);

        var messages = _messageRepo.Query().Where(m => m.PropertyId == property.Id).ToList();
        foreach (var msg in messages)
            _messageRepo.Delete(msg);

        var offers = _offerRepo.Query().Where(o => o.PropertyId == property.Id).ToList();
        foreach (var offer in offers)
            _offerRepo.Delete(offer);

        var favorites = _favoriteRepo.Query().Where(f => f.PropertyId == property.Id).ToList();
        foreach (var fav in favorites)
            _favoriteRepo.Delete(fav);

        _propertyRepo.Delete(property);
    }
}
