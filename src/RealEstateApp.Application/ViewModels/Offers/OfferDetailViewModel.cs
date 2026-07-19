namespace RealEstateApp.Application.ViewModels.Offers;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class OfferDetailViewModel : BaseViewModel
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string ClientName { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Status { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? RespondedAt { get; init; }
    public bool IsPending => Status == "Pending";
    public bool ShowActions => IsPending;
}
