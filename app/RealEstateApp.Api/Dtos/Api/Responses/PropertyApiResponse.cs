namespace RealEstateApp.Api.Dtos.Api.Responses;

public sealed class PropertyApiResponse
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string PropertyType { get; init; } = null!;
    public string SaleType { get; init; } = null!;
    public decimal Price { get; init; }
    public decimal LandSize { get; init; }
    public int Bedrooms { get; init; }
    public int Bathrooms { get; init; }
    public string? Description { get; init; }
    public List<ImprovementApiItem> Improvements { get; init; } = [];
    public string? AgentName { get; init; }
    public string? AgentId { get; init; }
    public string Status { get; init; } = null!;
}

public sealed class ImprovementApiItem
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}
