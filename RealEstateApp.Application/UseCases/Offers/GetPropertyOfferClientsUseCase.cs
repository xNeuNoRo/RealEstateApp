using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Offers;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Offers;

public sealed class GetPropertyOfferClientsUseCase : IGetPropertyOfferClientsUseCase
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetPropertyOfferClientsRequest> _validator;

    public GetPropertyOfferClientsUseCase(
        IOfferRepository offerRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetPropertyOfferClientsRequest> validator
    )
    {
        _offerRepository = offerRepository;
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PagedResult<OfferClientSummaryResponse>>> ExecuteAsync(
        GetPropertyOfferClientsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return validation.ToResult<PagedResult<OfferClientSummaryResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<OfferClientSummaryResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión para ver ofertas.")
            );
        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<PagedResult<OfferClientSummaryResponse>>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden ver ofertas.")
            );

        if (
            !await _propertyRepository.IsPropertyOwnedByAgentAsync(
                request.PropertyId,
                _currentUser.UserId,
                cancellationToken
            )
        )
            return Result<PagedResult<OfferClientSummaryResponse>>.Failure(
                Error.Forbidden(
                    "Offer.NotOwner",
                    "Solo el agente propietario puede ver las ofertas de esta propiedad."
                )
            );

        var summaries = await _offerRepository.GetClientSummariesByPropertyAsync(
            request.PropertyId,
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            cancellationToken
        );
        var users = await _userRepository.GetByIdsAsync(
            summaries.Select(summary => summary.ClientId).ToArray(),
            cancellationToken
        );
        var userMap = users.ToDictionary(user => user.Id);
        var items = _mapper.Map<List<OfferClientSummaryResponse>>(summaries);
        foreach (var item in items)
            if (userMap.TryGetValue(item.ClientId, out var user))
                item.ClientName = $"{user.FirstName} {user.LastName}".Trim();
            else
                item.ClientName = "Cliente no disponible";

        var totalCount = await _offerRepository.CountClientsByPropertyAsync(
            request.PropertyId,
            cancellationToken
        );
        return Result<PagedResult<OfferClientSummaryResponse>>.Success(
            new(items, totalCount, request.Page, request.PageSize)
        );
    }
}
