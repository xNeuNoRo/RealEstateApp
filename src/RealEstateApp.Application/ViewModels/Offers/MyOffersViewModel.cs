namespace RealEstateApp.Application.ViewModels.Offers;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class MyOffersViewModel : BaseViewModel
{
    public IReadOnlyList<OfferListItemViewModel> Offers { get; init; } = [];
}
