namespace RealEstateApp.Application.ViewModels.Offers;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class PropertyOffersViewModel : BaseViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public IReadOnlyList<OfferListItemViewModel> Offers { get; init; } = [];
}
