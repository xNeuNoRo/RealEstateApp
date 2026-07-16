using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Property;

public sealed class PropertySearchByCodeViewModel
{
    [Display(Name = "Código de propiedad")]
    [Required(ErrorMessage = "El código de propiedad es requerido.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código de propiedad debe ser de 6 dígitos.")]
    public string Code { get; set; } = null!;
}
