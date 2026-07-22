using FluentValidation;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Offers;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Offers;

public sealed class RejectOfferUseCase : IRejectOfferUseCase
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<RejectOfferRequest> _validator;
    private readonly ILogger<RejectOfferUseCase> _logger;

    public RejectOfferUseCase(
        IOfferRepository offerRepository,
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<RejectOfferRequest> validator,
        ILogger<RejectOfferUseCase> logger
    )
    {
        _offerRepository = offerRepository;
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        RejectOfferRequest request,
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
                    "Debe iniciar sesión para rechazar ofertas."
                )
            );

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden rechazar ofertas.")
            );

        var offer = await _offerRepository.GetByIdAsync(request.OfferId, cancellationToken);
        if (offer is null)
            return Result.Failure(
                Error.NotFound("Offer.NotFound", "No se encontró la oferta especificada.")
            );

        var property = await _propertyRepository.GetByIdAsync(offer.PropertyId, cancellationToken);
        if (property is null)
            return Result.Failure(
                Error.NotFound("Offer.PropertyNotFound", "No se encontró la propiedad asociada.")
            );

        if (property.AgentId != _currentUser.UserId)
            return Result.Failure(
                Error.Forbidden(
                    "Offer.NotOwner",
                    "Solo el agente propietario puede rechazar ofertas de esta propiedad."
                )
            );

        var rejectResult = offer.Reject();
        if (rejectResult.IsFailure)
            return Result.Failure(rejectResult.GetError());

        _offerRepository.Update(offer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Oferta {OfferId} rechazada por agente {AgentId}.",
            offer.Id,
            _currentUser.UserId
        );

        return Result.Success();
    }
}
