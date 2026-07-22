using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Dtos.Catalog.Requests;

public sealed record CreateSaleTypeRequest(SaleTypeCode Code, string Name, string Description);
