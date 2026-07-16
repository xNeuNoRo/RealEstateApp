using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Dtos.Offers.Requests;

public sealed record GetPropertyOffersRequest(
    int PropertyId,
    int Page = 1,
    int PageSize = 20,
    OfferStatus? Status = null
);
