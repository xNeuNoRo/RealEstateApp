namespace RealEstateApp.Application.Dtos.Property.Responses;

public sealed class PropertyDetailResponse
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
    public string? SaleTypeName { get; init; }
    public string? AgentId { get; init; }
    public string? AgentName { get; set; }
    public string? AgentPhone { get; set; }
    public string? AgentEmail { get; set; }
    public string? AgentProfileImage { get; set; }
    public IReadOnlyList<PropertyImageDto> Images { get; set; } = [];
    public IReadOnlyList<PropertyImprovementDto> Improvements { get; set; } = [];
}

public sealed class PropertyImageDto
{
    public int Id { get; init; }
    public string Url { get; init; } = null!;
    public bool IsMain { get; init; }
}

public sealed class PropertyImprovementDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}
