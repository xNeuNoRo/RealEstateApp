using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Dtos.Property.Requests;

public sealed record CreatePropertyRequest(
    string Description,
    decimal Price,
    string Currency,
    decimal SizeM2,
    int Bedrooms,
    int Bathrooms,
    int PropertyTypeId,
    int SaleTypeId,
    IReadOnlyList<IAppFile> ImageFiles,
    IReadOnlyList<int> ImprovementIds
);
