using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Property;

public sealed class PropertyFilterViewModel : IValidatableObject
{
    [Display(Name = "Tipo de propiedad")]
    public int? PropertyTypeId { get; set; }

    [Display(Name = "Tipo de venta")]
    public int? SaleTypeId { get; set; }

    [Display(Name = "Precio mínimo")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio mínimo no puede ser menor que cero.")]
    public decimal? PriceMin { get; set; }

    [Display(Name = "Precio máximo")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio máximo no puede ser menor que cero.")]
    public decimal? PriceMax { get; set; }

    [Display(Name = "Cantidad de habitaciones")]
    [Range(0, 100, ErrorMessage = "La cantidad de habitaciones no puede ser menor que cero.")]
    public int? Bedrooms { get; set; }

    [Display(Name = "Cantidad de baños")]
    [Range(0, 100, ErrorMessage = "La cantidad de baños no puede ser menor que cero.")]
    public int? Bathrooms { get; set; }

    [Display(Name = "Código de propiedad")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código de propiedad debe ser de 6 dígitos.")]
    public string? Code { get; set; }

    public IReadOnlyList<Shared.SelectListItemViewModel> PropertyTypes { get; set; } = [];
    public IReadOnlyList<Shared.SelectListItemViewModel> SaleTypes { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PriceMin.HasValue && PriceMax.HasValue && PriceMin > PriceMax)
        {
            yield return new ValidationResult(
                "El precio mínimo no puede ser mayor que el precio máximo.",
                [nameof(PriceMin), nameof(PriceMax)]
            );
        }
    }
}
