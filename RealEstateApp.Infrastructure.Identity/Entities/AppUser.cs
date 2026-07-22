using Microsoft.AspNetCore.Identity;
using RealEstateApp.Domain.Common;

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

    /// <summary>
    /// Establece el teléfono sincronizando la propiedad personalizada Phone
    /// con la heredada PhoneNumber de IdentityUser.
    /// </summary>
    public void SetPhone(string? phone)
    {
        Phone = phone;
        PhoneNumber = phone;
    }

    /// <summary>Ruta o URL de la imagen de perfil del usuario.</summary>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// Documento de identidad (cédula dominicana, 11 dígitos).
    /// </summary>
    public string? IdentityDocument { get; set; }

    /// <summary>Indica si el usuario puede iniciar sesión.</summary>
    public bool Active { get; set; } = true;

    /// <summary>Activa la cuenta del usuario</summary>
    public void Activate()
    {
        Active = true;
        UpdatedAt = DomainTime.UtcNow;
    }

    /// <summary>Desactiva la cuenta del usuario</summary>
    public void Deactivate()
    {
        Active = false;
        UpdatedAt = DomainTime.UtcNow;
    }

    /// <summary>Fecha de creación del registro.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DomainTime.UtcNow;

    /// <summary>Fecha de última actualización del registro.</summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>Devuelve el nombre completo del usuario.</summary>
    public string GetDisplayName() => $"{FirstName} {LastName}".Trim();
}
