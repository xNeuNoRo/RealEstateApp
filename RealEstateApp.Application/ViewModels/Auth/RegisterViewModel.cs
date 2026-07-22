using System.ComponentModel.DataAnnotations;
using RealEstateApp.Application.ViewModels.Shared;

namespace RealEstateApp.Application.ViewModels.Auth;

public sealed class RegisterViewModel : BaseViewModel
{
    [Required(ErrorMessage = "Selecciona el tipo de cuenta que deseas crear.")]
    [RegularExpression("^(Client|Agent)$", ErrorMessage = "El tipo de cuenta seleccionado no es válido.")]
    public string SelectedRole { get; set; } = "Client";

    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido.")]
    [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido.")]
    [RegularExpression(
        @"^(809|829|849)-\d{3}-\d{4}$",
        ErrorMessage = "Ingresa un teléfono dominicano válido (ej. 809-555-1234)."
    )]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es requerido.")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Debe tener entre 4 y 50 caracteres.")]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Debe tener al menos 8 caracteres.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$",
        ErrorMessage = "Incluye mayúscula, minúscula, número y símbolo."
    )]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Website { get; set; }
}
