using FluentValidation;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Domain.Interfaces.Services;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Application.UseCases.Property;

public sealed class CreatePropertyUseCase : ICreatePropertyUseCase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyCodeGenerator _codeGenerator;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreatePropertyRequest> _validator;
    private readonly ILogger<CreatePropertyUseCase> _logger;

    public CreatePropertyUseCase(
        IPropertyRepository propertyRepository,
        IPropertyCodeGenerator codeGenerator,
        IFileService fileService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<CreatePropertyRequest> validator,
        ILogger<CreatePropertyUseCase> logger
    )
    {
        _propertyRepository = propertyRepository;
        _codeGenerator = codeGenerator;
        _fileService = fileService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CreatePropertyResponse>> ExecuteAsync(
        CreatePropertyRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<CreatePropertyResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<CreatePropertyResponse>.Failure(
                Error.Unauthorized(
                    "Auth.NotAuthenticated",
                    "Debe iniciar sesión para realizar esta acción."
                )
            );

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<CreatePropertyResponse>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden publicar propiedades.")
            );

        var propertyCode = await _codeGenerator.GenerateNextAsync(cancellationToken);

        var price = Price.Create(request.Price, request.Currency);
        if (price.IsFailure)
            return Result<CreatePropertyResponse>.Failure(price.GetError());

        var size = Size.Create(request.SizeM2);
        if (size.IsFailure)
            return Result<CreatePropertyResponse>.Failure(size.GetError());

        foreach (var img in request.ImageFiles)
        {
            if (!_fileService.IsImageValid(img))
            {
                _logger.LogWarning("Imagen inválida en creación de propiedad.");
                return Result<CreatePropertyResponse>.Failure(
                    Error.Validation(
                        "Property.InvalidImage",
                        "Una de las imágenes no tiene un formato válido."
                    )
                );
            }

        }

        var imageUrls = new List<string>(request.ImageFiles.Count);
        string folder = $"{FileConstants.PropertiesFolder}/{propertyCode.Value}";
        Domain.Entities.Property property;
        try
        {
            foreach (var img in request.ImageFiles)
                imageUrls.Add(await _fileService.UploadFileAsync(img, folder));

            var propertyResult = Domain.Entities.Property.Create(
                propertyCode,
                request.Title,
                request.Description,
                price.GetValue(),
                size.GetValue(),
                request.Bedrooms,
                request.Bathrooms,
                request.PropertyTypeId,
                request.SaleTypeId,
                _currentUser.UserId,
                imageUrls,
                request.ImprovementIds
            );

            if (propertyResult.IsFailure)
            {
                DeleteUploadedFiles(imageUrls);
                return Result<CreatePropertyResponse>.Failure(propertyResult.GetError());
            }

            property = propertyResult.GetValue();
            await _propertyRepository.AddAsync(property, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            DeleteUploadedFiles(imageUrls);
            throw;
        }

        _logger.LogInformation(
            "Propiedad {Code} creada por agente {AgentId}.",
            propertyCode.Value,
            _currentUser.UserId
        );

        return Result<CreatePropertyResponse>.Success(
            new CreatePropertyResponse(
                property.Id,
                propertyCode.Value,
                "Propiedad publicada correctamente."
            )
        );
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
