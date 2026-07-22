namespace RealEstateApp.Application.ViewModels.Offers;

public sealed class PropertySummaryViewModel
{
    public int PropertyId { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string? PropertyTypeName { get; init; }
    public string? SaleTypeName { get; init; }
    public string? MainImageUrl { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "DOP";
}
