using System.ComponentModel.DataAnnotations;
using RealEstateApp.Application.ViewModels.Shared;

namespace RealEstateApp.Application.ViewModels.Auth;

public sealed class ForgotPasswordViewModel : BaseViewModel
{
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = null!;

    public string? Website { get; set; }
}
