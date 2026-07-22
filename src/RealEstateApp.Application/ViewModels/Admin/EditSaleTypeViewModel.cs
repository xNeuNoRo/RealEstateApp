using System.ComponentModel.DataAnnotations;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class EditSaleTypeViewModel : BaseViewModel
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "El código de tipo de venta es requerido.")]
    [Display(Name = "Código de tipo de venta")]
    public SaleTypeCode Code { get; set; } = SaleTypeCode.Sale;

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(
        80,
        MinimumLength = 2,
        ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres."
    )]
    [Display(Name = "Nombre del tipo de venta")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "La descripción es requerida.")]
    [StringLength(
        300,
        MinimumLength = 5,
        ErrorMessage = "La descripción debe tener entre 5 y 300 caracteres."
    )]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = null!;
}
