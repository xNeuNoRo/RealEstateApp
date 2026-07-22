namespace RealEstateApp.Application.Dtos.Offers.Requests;

public sealed record GetPropertyOfferClientsRequest(
    int PropertyId,
    int Page = 1,
    int PageSize = 12
);
