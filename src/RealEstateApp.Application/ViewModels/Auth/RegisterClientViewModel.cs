using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Auth;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class RegisterClientViewModel : BaseViewModel
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

    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [StringLength(
        50,
        MinimumLength = 4,
        ErrorMessage = "El nombre de usuario debe tener entre 4 y 50 caracteres."
    )]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres."
    )]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Debe confirmar su contraseña.")]
    [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = null!;
}
