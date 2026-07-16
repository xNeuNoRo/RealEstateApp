namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class OfferDetailViewModel
{
    public int OfferId { get; init; }
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string ClientName { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Status { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsPending => Status == "Pendiente";
}
