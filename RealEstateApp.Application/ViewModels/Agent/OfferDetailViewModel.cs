namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class OfferDetailViewModel : BaseViewModel
{
    public int OfferId { get; init; }
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string ClientName { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Status { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsPending => Status == "Pending";
}
