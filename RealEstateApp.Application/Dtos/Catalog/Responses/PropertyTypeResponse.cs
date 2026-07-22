namespace RealEstateApp.Application.Dtos.Catalog.Responses;

public sealed record PropertyTypeResponse(int Id, string Name, string Description)
{
    public int PropertiesCount { get; init; }
}
