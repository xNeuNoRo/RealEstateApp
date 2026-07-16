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
using PropertyEntity = RealEstateApp.Domain.Entities.Property;

namespace RealEstateApp.Application.UseCases.Offers;

public sealed class GetMyOffersUseCase : IGetMyOffersUseCase
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetMyOffersRequest> _validator;

    public GetMyOffersUseCase(
        IOfferRepository offerRepository,
        IPropertyRepository propertyRepository,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetMyOffersRequest> validator
    )
    {
        _offerRepository = offerRepository;
        _propertyRepository = propertyRepository;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PagedResult<OfferResponse>>> ExecuteAsync(
        GetMyOffersRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<OfferResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<OfferResponse>>.Failure(
                Error.Unauthorized(
                    "Auth.NotAuthenticated",
                    "Debe iniciar sesión para ver sus ofertas."
                )
            );

        if (!_currentUser.IsInRole(nameof(Roles.Client)))
            return Result<PagedResult<OfferResponse>>.Failure(
                Error.Forbidden("Auth.ClientOnly", "Solo los clientes pueden ver sus ofertas.")
            );

        var options = new QueryOptions<Offer>
        {
            OrderBy = q => q.OrderByDescending(o => o.CreatedAt),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        var offers = await _offerRepository.GetByClientAsync(
            _currentUser.UserId,
            options,
            cancellationToken
        );

        var totalCount = await _offerRepository.CountAsync(
            o => o.ClientId == _currentUser.UserId,
            cancellationToken
        );

        var propertyIds = offers.Select(o => o.PropertyId).Distinct().ToList();
        var properties =
            propertyIds.Count > 0
                ? await _propertyRepository.GetAllAsync(
                    new QueryOptions<PropertyEntity> { Filter = p => propertyIds.Contains(p.Id) },
                    cancellationToken
                )
                : [];
        var propertyMap = properties.ToDictionary(p => p.Id, p => p.Code.Value);

        var items = _mapper.Map<List<OfferResponse>>(offers);
        foreach (var item in items)
        {
            var offer = offers.First(o => o.Id == item.Id);
            item.PropertyCode = propertyMap.TryGetValue(offer.PropertyId, out var code)
                ? code
                : string.Empty;
            item.ClientName = _currentUser.FullName ?? string.Empty;
        }

        return Result<PagedResult<OfferResponse>>.Success(
            new PagedResult<OfferResponse>(items, totalCount, request.Page, request.PageSize)
        );
    }
}
