using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Auth;

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = null!;
}
