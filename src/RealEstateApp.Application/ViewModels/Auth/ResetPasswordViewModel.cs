using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Auth;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class ResetPasswordViewModel : BaseViewModel
{
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = null!;

    [Required]
    public string Token { get; set; } = null!;

    [Required(ErrorMessage = "La nueva contraseña es requerida.")]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres."
    )]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
    [Compare("NewPassword", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmPassword { get; set; } = null!;
}
