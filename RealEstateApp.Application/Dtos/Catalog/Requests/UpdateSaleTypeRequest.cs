using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Dtos.Catalog.Requests;

public sealed record UpdateSaleTypeRequest(
    int Id,
    SaleTypeCode Code,
    string Name,
    string Description
);
