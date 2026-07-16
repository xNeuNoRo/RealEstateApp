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

namespace RealEstateApp.Application.UseCases.Property;

public sealed class DeletePropertyUseCase : IDeletePropertyUseCase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IFileService _fileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<DeletePropertyRequest> _validator;
    private readonly ILogger<DeletePropertyUseCase> _logger;

    public DeletePropertyUseCase(
        IPropertyRepository propertyRepository,
        IFileService fileService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<DeletePropertyRequest> validator,
        ILogger<DeletePropertyUseCase> logger
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
        DeletePropertyRequest request,
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
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden eliminar propiedades.")
            );

        var property = await _propertyRepository.GetByIdAsync(
            request.PropertyId,
            cancellationToken,
            p => p.Images
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
                    "Solo el agente propietario puede eliminar esta propiedad."
                )
            );

        if (property.Status != PropertyStatus.Available)
            return Result.Failure(
                Error.Conflict(
                    "Property.NotAvailable",
                    "Solo se pueden eliminar propiedades disponibles."
                )
            );

        string folder = $"{FileConstants.PropertiesFolder}/{property.Code.Value}";
        foreach (var img in property.Images)
        {
            try
            {
                _fileService.DeleteFile(img.Url);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Error al eliminar imagen {Url} de propiedad {PropertyId}.",
                    img.Url,
                    property.Id
                );
            }
        }

        _propertyRepository.Delete(property);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Propiedad {PropertyId} eliminada por agente {AgentId}.",
            request.PropertyId,
            _currentUser.UserId
        );

        return Result.Success();
    }
}
