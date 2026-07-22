namespace RealEstateApp.Application.ViewModels.Offers;

using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;

public sealed class MyOffersViewModel : BaseViewModel
{
    public PagedResult<OfferListItemViewModel> Offers { get; init; } = new([], 0, 1, 12);
    public int? PropertyId { get; init; }
    public OfferStatus? Status { get; init; }
}
