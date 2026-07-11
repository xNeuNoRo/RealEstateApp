using AutoMapper;
using FluentValidation;
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

public sealed class CreateOfferUseCase : ICreateOfferUseCase
{
    private readonly IOfferPolicy _offerPolicy;
    private readonly IOfferRepository _offerRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateOfferRequest> _validator;

    public CreateOfferUseCase(
        IOfferPolicy offerPolicy,
        IOfferRepository offerRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<CreateOfferRequest> validator
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
    }

    public async Task<Result<OfferResponse>> ExecuteAsync(
        CreateOfferRequest request,
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
                    "Debe iniciar sesión para hacer una oferta."
                )
            );

        if (!_currentUser.IsInRole(nameof(Roles.Client)))
            return Result<OfferResponse>.Failure(
                Error.Forbidden("Auth.ClientOnly", "Solo los clientes pueden hacer ofertas.")
            );

        var policyResult = await _offerPolicy.CanCreateOfferAsync(
            request.PropertyId,
            _currentUser.UserId,
            cancellationToken
        );
        if (policyResult.IsFailure)
            return Result<OfferResponse>.Failure(policyResult.GetError());

        var offerResult = Domain.Entities.Offer.Create(
            request.PropertyId,
            _currentUser.UserId,
            request.Amount
        );
        if (offerResult.IsFailure)
            return Result<OfferResponse>.Failure(offerResult.GetError());

        var offer = offerResult.GetValue();
        await _offerRepository.AddAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var property = await _propertyRepository.GetByIdAsync(offer.PropertyId, cancellationToken);

        var user = await _userRepository.GetByIdAsync(offer.ClientId, cancellationToken);

        var response = _mapper.Map<OfferResponse>(offer);
        response.PropertyCode = property?.Code.Value ?? string.Empty;
        response.ClientName = user is not null
            ? $"{user.FirstName} {user.LastName}".Trim()
            : _currentUser.FullName ?? string.Empty;

        return Result<OfferResponse>.Success(response);
    }
}
