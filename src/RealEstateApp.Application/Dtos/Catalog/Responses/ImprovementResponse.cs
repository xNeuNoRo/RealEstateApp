namespace RealEstateApp.Application.Dtos.Catalog.Responses;

public sealed record ImprovementResponse(int Id, string Name, string Description)
{
    public int PropertiesCount { get; init; }
}
