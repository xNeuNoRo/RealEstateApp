using Microsoft.AspNetCore.Identity;

namespace RealEstateApp.Infrastructure.Identity.Entities;

/// <summary>
/// Extiende IdentityUser para incluir propiedades adicionales específicas de la app.
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>Nombre del usuario.</summary>
    public required string FirstName { get; set; }

    /// <summary>Apellido del usuario.</summary>
    public required string LastName { get; set; }

    /// <summary>Teléfono de contacto del usuario.</summary>
    public string? Phone { get; set; }

    /// <summary>Ruta o URL de la imagen de perfil del usuario.</summary>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// Documento de identidad (cédula dominicana, 11 dígitos).
    /// </summary>
    public string? IdentityDocument { get; set; }

    /// <summary>Indica si el usuario puede iniciar sesión.</summary>
    public bool Active { get; set; } = true;

    /// <summary>Devuelve el nombre completo del usuario.</summary>
    public string GetDisplayName() => $"{FirstName} {LastName}".Trim();
}
