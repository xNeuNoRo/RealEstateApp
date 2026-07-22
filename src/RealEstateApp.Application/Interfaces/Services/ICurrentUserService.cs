using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Interfaces.Services;

/// <summary>
/// Proporciona acceso a la identidad e información del usuario autenticado en la
/// sesión/request actual.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID del usuario autenticado (string, formato Identity).
    /// <c>null</c> si no hay sesión.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// UserName de Identity. <c>null</c> si no hay sesión.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Correo electrónico del usuario. <c>null</c> si no hay sesión.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Nombre completo (FirstName + LastName) del usuario. <c>null</c> si no hay sesión.
    /// </summary>
    string? FullName { get; }

    /// <summary>
    /// Ruta o URL de la imagen de perfil. <c>null</c> si no hay sesión.
    /// </summary>
    string? ProfilePicturePath { get; }

    /// <summary>
    /// Indica si hay una sesión activa (cualquier usuario autenticado).
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Roles asignados al usuario (<c>Client</c>, <c>Agent</c>,
    /// <c>Admin</c>, <c>Developer</c>).
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Indica si el usuario pertenece al rol indicado. Case-sensitive (usa
    /// <see cref="Roles"/> que vienen de Identity; comparar con
    /// <see cref="Roles.Client"/> string value).
    /// <example>
    /// <code>
    /// if (!_currentUser.IsInRole(nameof(Roles.Agent))) return Result.Forbidden(...);
    /// </code>
    /// </example>
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Indica si la cuenta del usuario está activa (campo <c>Active</c> de <c>AppUser</c>).
    /// Requiere consulta a BD, por eso es async. Devuelve <c>false</c> si no hay sesión.
    /// </summary>
    Task<bool> IsActiveAsync(CancellationToken cancellationToken = default);
}
