using System.Collections.ObjectModel;

namespace RealEstateApp.Domain.Settings;

/// <summary>
/// Constantes centralizadas para configuración de archivos e imágenes.
/// </summary>
public static class FileConstants
{
    /// <summary>
    /// Tamaño máximo permitido para imágenes en bytes (15 MB).
    /// </summary>
    public const long MaxImageFileSizeBytes = 15 * 1024 * 1024;

    /// <summary>
    /// Extensiones de imagen permitidas.
    /// </summary>
    public static readonly IReadOnlySet<string> AllowedImageExtensions = new ReadOnlySet<string>(
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" }
    );

    /// <summary>
    /// Tipos MIME permitidos para imágenes.
    /// </summary>
    public static readonly IReadOnlyList<string> AllowedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    /// <summary>Subcarpeta para imágenes de propiedades.</summary>
    public const string PropertiesFolder = "properties";

    /// <summary>Subcarpeta para fotos de perfil de usuarios.</summary>
    public const string ProfilesFolder = "profiles";

    /// <summary>Subcarpeta para archivos temporales.</summary>
    public const string TempFolder = "temp";
}
