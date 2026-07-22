using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Dtos.Offers.Requests;

public sealed record GetMyOffersRequest(
    int Page = 1,
    int PageSize = 20,
    int? PropertyId = null,
    OfferStatus? Status = null
);
