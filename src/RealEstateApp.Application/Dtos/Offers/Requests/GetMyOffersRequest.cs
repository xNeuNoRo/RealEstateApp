namespace RealEstateApp.Application.Dtos.Offers.Requests;

public sealed record GetMyOffersRequest(int Page = 1, int PageSize = 20);
