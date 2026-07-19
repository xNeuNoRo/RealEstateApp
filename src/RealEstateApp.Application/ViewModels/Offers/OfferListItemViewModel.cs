namespace RealEstateApp.Application.ViewModels.Offers;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class OfferListItemViewModel : BaseViewModel
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string PropertyDescription { get; init; } = null!;
    public string? PropertyTypeName { get; init; }
    public string? SaleTypeName { get; init; }
    public string? PropertyMainImageUrl { get; init; }
    public decimal PropertyPrice { get; init; }
    public string PropertyCurrency { get; init; } = "DOP";
    public string PropertyStatus { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string ClientName { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Status { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? RespondedAt { get; init; }
    public string StatusLabel =>
        Status switch
        {
            "Pending" => "Pendiente",
            "Accepted" => "Aceptada",
            "Rejected" => "Rechazada",
            _ => Status,
        };
    public bool IsPending => Status == "Pending";
    public bool IsAccepted => Status == "Accepted";
    public bool IsRejected => Status == "Rejected";
}
