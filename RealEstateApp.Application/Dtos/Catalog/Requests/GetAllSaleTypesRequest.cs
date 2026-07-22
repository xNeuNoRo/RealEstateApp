namespace RealEstateApp.Application.Dtos.Catalog.Requests;

public sealed record GetAllSaleTypesRequest(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);
