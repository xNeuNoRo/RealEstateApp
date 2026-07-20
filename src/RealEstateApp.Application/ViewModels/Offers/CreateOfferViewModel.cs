using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Offers;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class CreateOfferViewModel : BaseViewModel
{
    public int PropertyId { get; set; }

    public string PropertyCode { get; set; } = null!;
    public string PropertyTitle { get; set; } = null!;
    public string PropertyDescription { get; set; } = null!;
    public string? PropertyTypeName { get; set; }
    public string? SaleTypeName { get; set; }
    public string? PropertyMainImageUrl { get; set; }
    public decimal PropertyPrice { get; set; }
    public string PropertyCurrency { get; set; } = "DOP";

    [Required(ErrorMessage = "Debe ingresar el monto de la oferta.")]
    [Range(
        0.01,
        100_000_000,
        ErrorMessage = "El monto de la oferta debe ser un valor numérico mayor que cero."
    )]
    [Display(Name = "Monto ofertado")]
    public decimal Amount { get; set; }
}
