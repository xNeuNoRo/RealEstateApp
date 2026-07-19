using RealEstateApp.Application.Dtos.Property.Responses;

namespace RealEstateApp.Application.ViewModels.Property;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class PropertyDetailPublicViewModel : BaseViewModel
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public decimal SizeM2 { get; init; }
    public int Bedrooms { get; init; }
    public int Bathrooms { get; init; }
    public string Status { get; init; } = null!;
    public string? MainImageUrl { get; init; }
    public string? PropertyTypeName { get; init; }
    public int PropertyTypeId { get; init; }
    public string? SaleTypeName { get; init; }
    public int SaleTypeId { get; init; }
    public string? AgentId { get; init; }
    public string? AgentName { get; init; }
    public string? AgentPhone { get; init; }
    public string? AgentEmail { get; init; }
    public string? AgentProfileImage { get; init; }
    public IReadOnlyList<PropertyImageDto> Images { get; init; } = [];
    public IReadOnlyList<PropertyImprovementDto> Improvements { get; init; } = [];
    public bool IsFavorite { get; init; }
}
