using System.ComponentModel.DataAnnotations;
using RealEstateApp.Application.ViewModels.Shared;

namespace RealEstateApp.Application.ViewModels.Auth;

public sealed class ResendActivationViewModel : BaseViewModel
{
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;
}
