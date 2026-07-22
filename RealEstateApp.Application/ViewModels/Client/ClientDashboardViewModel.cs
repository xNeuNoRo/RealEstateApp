namespace RealEstateApp.Application.ViewModels.Client;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class ClientDashboardViewModel : BaseViewModel
{
    public int FavoritesCount { get; init; }
    public int TotalOffers { get; init; }
    public int ActiveOffers { get; init; }
    public int ConversationsCount { get; init; }
    public int TotalMessages { get; init; }
    public IReadOnlyList<Property.PropertyListItemViewModel> AvailableProperties { get; init; } =
    [];
}
