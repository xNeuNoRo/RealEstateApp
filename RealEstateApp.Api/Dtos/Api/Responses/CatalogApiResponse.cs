namespace RealEstateApp.Api.Dtos.Api.Responses;

public sealed class CatalogApiResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int PropertiesCount { get; init; }
}

public sealed class SaleTypeApiResponse
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}
