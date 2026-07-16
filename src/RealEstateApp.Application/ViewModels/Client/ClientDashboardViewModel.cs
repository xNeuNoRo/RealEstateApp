namespace RealEstateApp.Application.ViewModels.Client;

public sealed class ClientDashboardViewModel
{
    public int FavoritesCount { get; init; }
    public int TotalOffers { get; init; }
    public int ActiveOffers { get; init; }
    public int ConversationsCount { get; init; }
    public int TotalMessages { get; init; }
    public IReadOnlyList<Property.PropertyListItemViewModel> AvailableProperties { get; init; } = [];
}
