namespace RealEstateApp.Application.ViewModels.Offers;

using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;

public sealed class PropertyOffersViewModel : BaseViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string PropertyTitle { get; init; } = null!;
    public string PropertyDescription { get; init; } = null!;
    public string? PropertyTypeName { get; init; }
    public string? PropertyMainImageUrl { get; init; }
    public decimal PropertyPrice { get; init; }
    public string PropertyCurrency { get; init; } = "DOP";
    public string PropertyStatus { get; init; } = null!;
    public string? ClientId { get; init; }
    public string? ClientName { get; init; }
    public OfferStatus? Status { get; init; }
    public PagedResult<OfferListItemViewModel> Offers { get; init; } = new([], 0, 1, 12);
    public PagedResult<OfferClientSummaryViewModel> Clients { get; init; } = new([], 0, 1, 12);
}
