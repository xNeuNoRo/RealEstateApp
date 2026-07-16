using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Auth;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Debe ingresar su correo o nombre de usuario.")]
    [Display(Name = "Correo o nombre de usuario")]
    public string UserNameOrEmail { get; set; } = null!;

    [Required(ErrorMessage = "Debe ingresar su contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;
}
