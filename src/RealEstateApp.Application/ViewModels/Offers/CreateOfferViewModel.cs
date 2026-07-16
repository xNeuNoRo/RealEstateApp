using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Offers;

public sealed class CreateOfferViewModel
{
    public int PropertyId { get; set; }

    [Required(ErrorMessage = "Debe ingresar el monto de la oferta.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto de la oferta debe ser un valor numérico mayor que cero.")]
    [Display(Name = "Monto ofertado")]
    public decimal Amount { get; set; }
}
