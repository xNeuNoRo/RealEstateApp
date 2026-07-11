namespace RealEstateApp.Application.Dtos.Offers.Responses;

public sealed class OfferResponse
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string PropertyCode { get; set; } = null!;
    public string ClientId { get; init; } = null!;
    public string ClientName { get; set; } = null!;
    public decimal Amount { get; init; }
    public string Status { get; init; } = null!;
    public DateTimeOffset? RespondedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
