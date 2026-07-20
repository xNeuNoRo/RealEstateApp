namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class AgentPropertyListItemViewModel : BaseViewModel
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public decimal SizeM2 { get; init; }
    public int Bedrooms { get; init; }
    public int Bathrooms { get; init; }
    public string Status { get; init; } = null!;
    public string? MainImageUrl { get; init; }
    public string? PropertyTypeName { get; init; }
    public string? SaleTypeName { get; init; }
    public bool IsSold => Status == "Vendida";
}
