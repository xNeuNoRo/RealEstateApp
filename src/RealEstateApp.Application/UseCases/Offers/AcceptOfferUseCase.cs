using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Offers;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Domain.Interfaces.Services;

namespace RealEstateApp.Application.UseCases.Offers;

public sealed class AcceptOfferUseCase : IAcceptOfferUseCase
{
    private readonly IOfferPolicy _offerPolicy;
    private readonly IOfferRepository _offerRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<AcceptOfferRequest> _validator;
    private readonly ILogger<AcceptOfferUseCase> _logger;

    public AcceptOfferUseCase(
        IOfferPolicy offerPolicy,
        IOfferRepository offerRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<AcceptOfferRequest> validator,
        ILogger<AcceptOfferUseCase> logger
    )
    {
        _offerPolicy = offerPolicy;
        _offerRepository = offerRepository;
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<OfferResponse>> ExecuteAsync(
        AcceptOfferRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<OfferResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<OfferResponse>.Failure(
                Error.Unauthorized(
                    "Auth.NotAuthenticated",
                    "Debe iniciar sesión para aceptar ofertas."
                )
            );

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<OfferResponse>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden aceptar ofertas.")
            );

        var offer = await _offerRepository.GetByIdAsync(request.OfferId, cancellationToken);
        if (offer is null)
            return Result<OfferResponse>.Failure(
                Error.NotFound("Offer.NotFound", "No se encontró la oferta especificada.")
            );

        var property = await _propertyRepository.GetByIdAsync(offer.PropertyId, cancellationToken);
        if (property is null)
            return Result<OfferResponse>.Failure(
                Error.NotFound("Offer.PropertyNotFound", "No se encontró la propiedad asociada.")
            );

        if (property.AgentId != _currentUser.UserId)
            return Result<OfferResponse>.Failure(
                Error.Forbidden(
                    "Offer.NotOwner",
                    "Solo el agente propietario puede aceptar ofertas de esta propiedad."
                )
            );

        var policyResult = await _offerPolicy.CanAcceptOfferAsync(
            offer.PropertyId,
            offer.Id,
            cancellationToken
        );
        if (policyResult.IsFailure)
            return Result<OfferResponse>.Failure(policyResult.GetError());

        var acceptResult = offer.Accept();
        if (acceptResult.IsFailure)
            return Result<OfferResponse>.Failure(acceptResult.GetError());

        var txResult = await _unitOfWork.ExecuteInTransactionAsync(
            async (ct) =>
            {
                _offerRepository.Update(offer);

                try
                {
                    await _unitOfWork.SaveChangesAsync(ct);
                }
                catch
                {
                    if (await _offerRepository.HasAcceptedOfferAsync(offer.PropertyId, ct))
                        return Result.Failure(
                            Error.Conflict(
                                "Offer.PropertyHasAcceptedOffer",
                                "La propiedad ya tiene una oferta aceptada."
                            )
                        );

                    throw;
                }

                return Result.Success();
            },
            ct: cancellationToken
        );

        if (txResult.IsFailure)
            return Result<OfferResponse>.Failure(txResult.GetError());

        _logger.LogInformation(
            "Oferta {OfferId} aceptada por agente {AgentId}. Propiedad {PropertyId}.",
            offer.Id,
            _currentUser.UserId,
            offer.PropertyId
        );

        var user = await _userRepository.GetByIdAsync(offer.ClientId, cancellationToken);

        var response = _mapper.Map<OfferResponse>(offer);
        response.PropertyCode = property.Code.Value;
        response.PropertyDescription = property.Description;
        response.PropertyPrice = property.Price.Amount;
        response.PropertyCurrency = property.Price.Currency;
        response.PropertyStatus = nameof(PropertyStatus.Sold);
        response.ClientName = user is not null
            ? $"{user.FirstName} {user.LastName}".Trim()
            : string.Empty;

        return Result<OfferResponse>.Success(response);
    }
}
