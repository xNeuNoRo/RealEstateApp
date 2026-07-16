namespace RealEstateApp.Application.ViewModels.Offers;

public sealed class OfferListItemViewModel
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
    public string StatusLabel => Status switch
    {
        "Pendiente" => "Pendiente",
        "Aceptada" => "Aceptada",
        "Rechazada" => "Rechazada",
        _ => Status
    };
    public bool IsPending => Status == "Pendiente";
}
