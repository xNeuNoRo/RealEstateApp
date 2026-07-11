namespace RealEstateApp.Application.Dtos.Property.Responses;

public sealed class PropertySummaryResponse
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string? MainImageUrl { get; init; }
}
