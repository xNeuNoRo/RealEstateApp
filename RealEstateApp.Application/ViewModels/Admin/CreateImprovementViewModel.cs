using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class CreateImprovementViewModel : BaseViewModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres."
    )]
    [Display(Name = "Nombre de la mejora")]
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
