namespace RealEstateApp.Application.ViewModels.Offers;

public sealed class OfferClientSummaryViewModel
{
    public string ClientId { get; init; } = null!;
    public string ClientName { get; init; } = null!;
    public int OfferCount { get; init; }
    public decimal LastAmount { get; init; }
    public string LastStatus { get; init; } = null!;
    public DateTimeOffset LastCreatedAt { get; init; }
    public string LastStatusLabel =>
        LastStatus switch
        {
            "Accepted" => "Aceptada",
            "Rejected" => "Rechazada",
            _ => "Pendiente",
        };
}
