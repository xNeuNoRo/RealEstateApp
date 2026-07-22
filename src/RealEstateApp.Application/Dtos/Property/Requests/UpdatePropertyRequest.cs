using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Dtos.Property.Requests;

public sealed record UpdatePropertyRequest(
    int PropertyId,
    string? Title = null,
    string? Description = null,
    decimal? Price = null,
    string? Currency = null,
    decimal? SizeM2 = null,
    int? Bedrooms = null,
    int? Bathrooms = null,
    int? PropertyTypeId = null,
    int? SaleTypeId = null,
    IReadOnlyList<IAppFile>? NewImageFiles = null,
    IReadOnlyList<int>? ImageIdsToRemove = null,
    IReadOnlyList<int>? ImprovementIdsToAdd = null,
    IReadOnlyList<int>? ImprovementIdsToRemove = null
);
