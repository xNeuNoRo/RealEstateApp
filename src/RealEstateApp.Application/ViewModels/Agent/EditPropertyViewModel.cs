using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class EditPropertyViewModel : BaseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El tipo de propiedad es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de propiedad.")]
    [Display(Name = "Tipo de propiedad")]
    public int PropertyTypeId { get; set; }

    [Required(ErrorMessage = "El tipo de venta es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de venta.")]
    [Display(Name = "Tipo de venta")]
    public int SaleTypeId { get; set; }

    [Required(ErrorMessage = "El precio es requerido.")]
    [Range(0.01, 100000000, ErrorMessage = "El precio debe ser mayor que cero.")]
    [Display(Name = "Precio")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "La descripción es requerida.")]
    [StringLength(
        2000,
        MinimumLength = 5,
        ErrorMessage = "La descripción debe tener entre 5 y 2000 caracteres."
    )]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "El tamaño es requerido.")]
    [Range(0.01, 10000, ErrorMessage = "El tamaño debe ser mayor que cero.")]
    [Display(Name = "Tamaño (m²)")]
    public decimal SizeM2 { get; set; }

    [Required(ErrorMessage = "La cantidad de habitaciones es requerida.")]
    [Range(0, 20, ErrorMessage = "Las habitaciones deben estar entre 0 y 20.")]
    [Display(Name = "Habitaciones")]
    public int Bedrooms { get; set; }

    [Required(ErrorMessage = "La cantidad de baños es requerida.")]
    [Range(0, 20, ErrorMessage = "Los baños deben estar entre 0 y 20.")]
    [Display(Name = "Baños")]
    public int Bathrooms { get; set; }

    [Required(ErrorMessage = "La moneda es requerida.")]
    [StringLength(
        3,
        MinimumLength = 3,
        ErrorMessage = "La moneda debe tener 3 caracteres (ej. DOP, USD)."
    )]
    [Display(Name = "Moneda")]
    public string Currency { get; set; } = "DOP";

    public string Code { get; set; } = null!;

    public IReadOnlyList<string> ExistingImageUrls { get; set; } = [];

    [Required(ErrorMessage = "Debe seleccionar al menos una mejora.")]
    [MinLength(1, ErrorMessage = "Debe seleccionar al menos una mejora.")]
    [Display(Name = "Mejoras")]
    public List<int> ImprovementIds { get; set; } = [];

    public IReadOnlyList<Shared.SelectListItemViewModel> PropertyTypes { get; set; } = [];
    public IReadOnlyList<Shared.SelectListItemViewModel> SaleTypes { get; set; } = [];
    public IReadOnlyList<Shared.SelectListItemViewModel> Improvements { get; set; } = [];
}
