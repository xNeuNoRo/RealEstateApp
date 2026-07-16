namespace RealEstateApp.Application.ViewModels.Offers;

public sealed class MyOffersViewModel
{
    public IReadOnlyList<OfferListItemViewModel> Offers { get; init; } = [];
}
