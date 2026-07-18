namespace RealEstateApp.Application.ViewModels.Client;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class MyFavoritesViewModel : BaseViewModel
{
    public IReadOnlyList<FavoriteListItemViewModel> Favorites { get; init; } = [];
}

public sealed class FavoriteListItemViewModel : BaseViewModel
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string Code { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public string? MainImageUrl { get; init; }
    public string? PropertyTypeName { get; init; }
    public string? SaleTypeName { get; init; }
    public string? AgentName { get; init; }
    public DateTimeOffset FavoritedAt { get; init; }
}
