namespace RealEstateApp.Application.Dtos.Favorites.Responses;

public sealed class FavoriteResponse
{
    public int Id { get; set; }
    public int PropertyId { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public string? MainImageUrl { get; init; }
    public string? PropertyTypeName { get; set; }
    public string? SaleTypeName { get; set; }
    public string? AgentName { get; set; }
    public DateTimeOffset FavoritedAt { get; set; }
}
