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

        var newImages = request.NewImageFiles ?? [];
        var imageIdsToRemove = request.ImageIdsToRemove?.Distinct().ToArray() ?? [];
        var existingImageIds = property.Images.Select(image => image.Id).ToHashSet();
        if (imageIdsToRemove.Any(id => !existingImageIds.Contains(id)))
            return Result.Failure(
                Error.NotFound("Property.Image", "Una de las imágenes seleccionadas no existe.")
            );

        var finalImageCount = property.Images.Count - imageIdsToRemove.Length + newImages.Count;
        if (finalImageCount is < 1 or > 4)
            return Result.Failure(
                Error.Validation(
                    "Property.Images",
                    finalImageCount < 1
                        ? "La propiedad debe mantener al menos una imagen."
                        : "La propiedad no puede tener más de 4 imágenes."
                )
            );

        foreach (var image in newImages)
        {
            if (!_fileService.IsImageValid(image))
                return Result.Failure(
                    Error.Validation(
                        "Property.InvalidImage",
                        "Una de las imágenes no tiene un formato válido."
                    )
                );
        }

        var improvementsToAdd = request.ImprovementIdsToAdd?.Distinct().ToArray() ?? [];
        var improvementsToRemove = request.ImprovementIdsToRemove?.Distinct().ToArray() ?? [];
        if (improvementsToAdd.Any(id => id <= 0) || improvementsToRemove.Any(id => id <= 0))
            return Result.Failure(
                Error.Validation("Property.ImprovementId", "Una de las mejoras no es válida.")
            );
        var existingImprovementIds = property.Improvements
            .Select(improvement => improvement.ImprovementId)
            .ToHashSet();
        if (improvementsToRemove.Any(id => !existingImprovementIds.Contains(id)))
            return Result.Failure(
                Error.NotFound("Property.Improvement", "Una de las mejoras seleccionadas no existe.")
            );
        if (improvementsToAdd.Any(existingImprovementIds.Contains))
            return Result.Failure(
                Error.Conflict("Property.DuplicateImprovement", "La mejora ya está vinculada.")
            );
        if (existingImprovementIds.Count - improvementsToRemove.Length + improvementsToAdd.Length < 1)
            return Result.Failure(
                Error.Validation("Property.Improvements", "Debe mantener al menos una mejora.")
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

        var removedImageUrls = new List<string>(imageIdsToRemove.Length);
        var uploadedImageUrls = new List<string>(newImages.Count);
        var remainingImageIdsToRemove = new Queue<int>(imageIdsToRemove);

        try
        {
            while (
                property.Images.Count + newImages.Count > 4
                && remainingImageIdsToRemove.Count > 0
            )
            {
                var imageId = remainingImageIdsToRemove.Dequeue();
                removedImageUrls.Add(property.Images.First(image => image.Id == imageId).Url);
                var removeResult = property.RemoveImage(imageId);
                if (removeResult.IsFailure)
                    return Result.Failure(removeResult.GetError());
            }

            if (newImages.Count > 0)
            {
                string folder = $"{FileConstants.PropertiesFolder}/{property.Code.Value}";
                foreach (var image in newImages)
                {
                    var url = await _fileService.UploadFileAsync(image, folder);
                    uploadedImageUrls.Add(url);
                    var addResult = property.AddImage(url);
                    if (addResult.IsFailure)
                    {
                        DeleteUploadedFiles(uploadedImageUrls);
                        return Result.Failure(addResult.GetError());
                    }
                }
            }

            while (remainingImageIdsToRemove.Count > 0)
            {
                var imageId = remainingImageIdsToRemove.Dequeue();
                removedImageUrls.Add(property.Images.First(image => image.Id == imageId).Url);
                var removeResult = property.RemoveImage(imageId);
                if (removeResult.IsFailure)
                {
                    DeleteUploadedFiles(uploadedImageUrls);
                    return Result.Failure(removeResult.GetError());
                }
            }

            foreach (var improvementId in improvementsToAdd)
            {
                var addResult = property.AddImprovement(improvementId);
                if (addResult.IsFailure)
                {
                    DeleteUploadedFiles(uploadedImageUrls);
                    return Result.Failure(addResult.GetError());
                }
            }

            foreach (var improvementId in improvementsToRemove)
            {
                var removeResult = property.RemoveImprovement(improvementId);
                if (removeResult.IsFailure)
                {
                    DeleteUploadedFiles(uploadedImageUrls);
                    return Result.Failure(removeResult.GetError());
                }
            }

            _propertyRepository.Update(property);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            DeleteUploadedFiles(uploadedImageUrls);
            throw;
        }

        foreach (var imageUrl in removedImageUrls)
        {
            try
            {
                _fileService.DeleteFile(imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar la imagen reemplazada {Url}.", imageUrl);
            }
        }

        _logger.LogInformation(
            "Propiedad {PropertyId} actualizada por agente {AgentId}.",
            request.PropertyId,
            _currentUser.UserId
        );

        return Result.Success();
    }

    private void DeleteUploadedFiles(IEnumerable<string> urls)
    {
        foreach (var url in urls)
        {
            try
            {
                _fileService.DeleteFile(url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo limpiar la imagen {Url}.", url);
            }
        }
    }
}
