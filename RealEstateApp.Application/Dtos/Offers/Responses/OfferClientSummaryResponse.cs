namespace RealEstateApp.Application.Dtos.Offers.Responses;

public sealed class OfferClientSummaryResponse
{
    public string ClientId { get; init; } = null!;
    public string ClientName { get; set; } = null!;
    public int OfferCount { get; init; }
    public decimal LastAmount { get; init; }
    public string LastStatus { get; init; } = null!;
    public DateTimeOffset LastCreatedAt { get; init; }
}
