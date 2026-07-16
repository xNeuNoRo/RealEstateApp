namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class OfferListViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public IReadOnlyList<Offers.OfferListItemViewModel> Offers { get; init; } = [];
}
