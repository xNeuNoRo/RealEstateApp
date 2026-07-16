namespace RealEstateApp.Application.ViewModels.Offers;

public sealed class PropertyOffersViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public IReadOnlyList<OfferListItemViewModel> Offers { get; init; } = [];
}
