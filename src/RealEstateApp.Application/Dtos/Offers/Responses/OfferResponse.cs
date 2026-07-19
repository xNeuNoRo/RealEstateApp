namespace RealEstateApp.Application.Dtos.Offers.Responses;

public sealed class OfferResponse
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string PropertyCode { get; set; } = null!;
    public string PropertyDescription { get; set; } = null!;
    public string? PropertyTypeName { get; set; }
    public string? SaleTypeName { get; set; }
    public string? PropertyMainImageUrl { get; set; }
    public decimal PropertyPrice { get; set; }
    public string PropertyCurrency { get; set; } = "DOP";
    public string PropertyStatus { get; set; } = null!;
    public string ClientId { get; init; } = null!;
    public string ClientName { get; set; } = null!;
    public decimal Amount { get; init; }
    public string Status { get; init; } = null!;
    public DateTimeOffset? RespondedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
