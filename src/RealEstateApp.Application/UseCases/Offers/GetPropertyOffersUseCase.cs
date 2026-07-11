using System.Linq.Expressions;
using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Offers;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Offers;

public sealed class GetPropertyOffersUseCase : IGetPropertyOffersUseCase
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetPropertyOffersRequest> _validator;

    public GetPropertyOffersUseCase(
        IOfferRepository offerRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetPropertyOffersRequest> validator
    )
    {
        _offerRepository = offerRepository;
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PagedResult<OfferResponse>>> ExecuteAsync(
        GetPropertyOffersRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<OfferResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<OfferResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión para ver ofertas.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<PagedResult<OfferResponse>>.Failure(
                Error.Forbidden(
                    "Auth.AgentOnly",
                    "Solo los agentes pueden ver ofertas de propiedades."
                )
            );

        var isOwner = await _propertyRepository.IsPropertyOwnedByAgentAsync(
            request.PropertyId,
            _currentUser.UserId,
            cancellationToken
        );
        if (!isOwner)
            return Result<PagedResult<OfferResponse>>.Failure(
                Error.Forbidden(
                    "Offer.NotOwner",
                    "Solo el agente propietario puede ver las ofertas de esta propiedad."
                )
            );

        var options = new QueryOptions<Offer>
        {
            OrderBy = q => q.OrderByDescending(o => o.CreatedAt),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            options.Filter = o => o.PropertyId == request.PropertyId && o.Status == status;
        }

        var offers = await _offerRepository.GetByPropertyAsync(
            request.PropertyId,
            options,
            cancellationToken
        );

        Expression<Func<Offer, bool>> countFilter = request.Status.HasValue
            ? o => o.PropertyId == request.PropertyId && o.Status == request.Status.Value
            : o => o.PropertyId == request.PropertyId;

        var totalCount = await _offerRepository.CountAsync(countFilter, cancellationToken);

        var property = await _propertyRepository.GetByIdAsync(
            request.PropertyId,
            cancellationToken
        );

        var clientIds = offers.Select(o => o.ClientId).Distinct().ToList();
        var users =
            clientIds.Count > 0
                ? await _userRepository.GetByIdsAsync(clientIds, cancellationToken)
                : [];
        var userMap = users.ToDictionary(u => u.Id);

        var items = _mapper.Map<List<OfferResponse>>(offers);
        foreach (var item in items)
        {
            var offer = offers.First(o => o.Id == item.Id);
            item.PropertyCode = property?.Code.Value ?? string.Empty;
            if (userMap.TryGetValue(offer.ClientId, out var user))
                item.ClientName = $"{user.FirstName} {user.LastName}".Trim();
        }

        return Result<PagedResult<OfferResponse>>.Success(
            new PagedResult<OfferResponse>(items, totalCount, request.Page, request.PageSize)
        );
    }
}
