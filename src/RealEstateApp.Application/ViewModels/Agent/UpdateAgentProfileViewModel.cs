using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class UpdateAgentProfileViewModel : BaseViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es requerido.")]
    [RegularExpression(
        @"^(809|829|849)-\d{3}-\d{4}$",
        ErrorMessage = "Debe ingresar un número telefónico válido de República Dominicana (ej. 809-555-1234)."
    )]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = null!;
}
