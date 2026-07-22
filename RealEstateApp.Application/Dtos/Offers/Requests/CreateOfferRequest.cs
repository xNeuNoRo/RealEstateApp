namespace RealEstateApp.Application.Dtos.Offers.Requests;

public sealed record CreateOfferRequest(int PropertyId, decimal Amount);
