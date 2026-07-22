using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Auth;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class ChangePasswordViewModel : BaseViewModel
{
    [Required(ErrorMessage = "La contraseña actual es requerida.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string CurrentPassword { get; set; } = null!;

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
