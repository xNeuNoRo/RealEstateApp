namespace RealEstateApp.Application.Dtos.Property.Requests;

public sealed record GetPropertyListRequest(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    decimal? SizeMin = null,
    decimal? SizeMax = null,
    int? Bedrooms = null,
    int? Bathrooms = null,
    int? PropertyTypeId = null,
    int? SaleTypeId = null,
    string? AgentId = null,
    string? Code = null,
    bool IncludeAllStatuses = false
);
