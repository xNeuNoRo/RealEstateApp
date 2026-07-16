using FluentValidation;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Application.UseCases.Property;

public sealed class UpdatePropertyUseCase : IUpdatePropertyUseCase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<UpdatePropertyRequest> _validator;
    private readonly ILogger<UpdatePropertyUseCase> _logger;

    public UpdatePropertyUseCase(
        IPropertyRepository propertyRepository,
        IFileService fileService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<UpdatePropertyRequest> validator,
        ILogger<UpdatePropertyUseCase> logger
    )
    {
        _propertyRepository = propertyRepository;
        _fileService = fileService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        UpdatePropertyRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.NotAuthenticated",
                    "Debe iniciar sesión para realizar esta acción."
                )
            );

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden editar propiedades.")
            );

        var property = await _propertyRepository.GetByIdAsync(
            request.PropertyId,
            cancellationToken,
            p => p.Images,
            p => p.Improvements
        );

        if (property is null)
            return Result.Failure(
                Error.NotFound("Property.NotFound", "No se encontró la propiedad especificada.")
            );

        var isOwner = await _propertyRepository.IsPropertyOwnedByAgentAsync(
            request.PropertyId,
            _currentUser.UserId,
            cancellationToken
        );
        if (!isOwner)
            return Result.Failure(
                Error.Forbidden(
                    "Property.NotOwner",
                    "Solo el agente propietario puede editar esta propiedad."
                )
            );

        if (property.Status != PropertyStatus.Available)
            return Result.Failure(
                Error.Conflict(
                    "Property.NotAvailable",
                    "Solo se pueden editar propiedades disponibles."
                )
            );

        Price? newPrice = null;
        if (request.Price.HasValue)
        {
            var currency = request.Currency ?? property.Price.Currency;
            var priceResult = Price.Create(request.Price.Value, currency);
            if (priceResult.IsFailure)
                return Result.Failure(priceResult.GetError());
            newPrice = priceResult.Value;
        }

        Size? newSize = null;
        if (request.SizeM2.HasValue)
        {
            var sizeResult = Size.Create(request.SizeM2.Value);
            if (sizeResult.IsFailure)
                return Result.Failure(sizeResult.GetError());
            newSize = sizeResult.Value;
        }

        var detailsResult = property.UpdateDetails(
            request.Description,
            newPrice,
            newSize,
            request.Bedrooms,
            request.Bathrooms,
            request.PropertyTypeId,
            request.SaleTypeId
        );

        if (detailsResult.IsFailure)
            return Result.Failure(detailsResult.GetError());

        if (request.NewImageFiles is { Count: > 0 })
        {
            string folder = $"{FileConstants.PropertiesFolder}/{property.Code.Value}";
            foreach (var img in request.NewImageFiles)
            {
                if (!_fileService.IsImageValid(img))
                    return Result.Failure(
                        Error.Validation(
                            "Property.InvalidImage",
                            "Una de las imágenes no tiene un formato válido."
                        )
                    );

                var url = await _fileService.UploadFileAsync(img, folder);
                var addResult = property.AddImage(url);
                if (addResult.IsFailure)
                    return Result.Failure(addResult.GetError());
            }
        }

        if (request.ImageIdsToRemove is { Count: > 0 })
        {
            foreach (var imageId in request.ImageIdsToRemove)
            {
                var removeResult = property.RemoveImage(imageId);
                if (removeResult.IsFailure && removeResult.GetError().Code != "Property.Image")
                    return Result.Failure(removeResult.GetError());
            }
        }

        if (request.ImprovementIdsToAdd is { Count: > 0 })
        {
            foreach (var impId in request.ImprovementIdsToAdd)
            {
                var addResult = property.AddImprovement(impId);
                if (addResult.IsFailure)
                    return Result.Failure(addResult.GetError());
            }
        }

        if (request.ImprovementIdsToRemove is { Count: > 0 })
        {
            foreach (var impId in request.ImprovementIdsToRemove)
            {
                var removeResult = property.RemoveImprovement(impId);
                if (removeResult.IsFailure)
                    return Result.Failure(removeResult.GetError());
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Propiedad {PropertyId} actualizada por agente {AgentId}.",
            request.PropertyId,
            _currentUser.UserId
        );

        return Result.Success();
    }
}
