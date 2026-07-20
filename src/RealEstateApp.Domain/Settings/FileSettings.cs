using System.Collections.ObjectModel;

namespace RealEstateApp.Domain.Settings;

/// <summary>
/// Configuración para el almacenamiento de archivos (imágenes, documentos).
/// </summary>
public sealed class FileSettings
{
    public const string SectionName = "FileSettings";

    /// <summary>Ruta base absoluta donde se almacenan los archivos físicos.</summary>
    public string BasePath { get; init; } = null!;

    /// <summary>Prefijo URL público para acceder a los archivos (ej: "/uploads").</summary>
    public string UrlPrefix { get; init; } = "/uploads";

    /// <summary>Tamaño máximo permitido para imágenes en bytes (15 MB).</summary>
    public long MaxImageFileSizeBytes { get; init; } = FileConstants.MaxImageFileSizeBytes;

    /// <summary>Extensiones de imagen permitidas (case-insensitive).</summary>
    public IReadOnlySet<string> AllowedImageExtensions { get; init; } =
        FileConstants.AllowedImageExtensions;

    /// <summary>Tipos MIME permitidos para imágenes.</summary>
    public IReadOnlyList<string> AllowedMimeTypes { get; init; } = FileConstants.AllowedMimeTypes;

    /// <summary>
    /// Verifica si una extensión o nombre de archivo tiene una extensión permitida.
    /// </summary>
    public bool IsAllowedImageExtension(string? extensionOrFileName)
    {
        if (string.IsNullOrWhiteSpace(extensionOrFileName))
            return false;

        var ext = NormalizeExtension(extensionOrFileName);
        return AllowedImageExtensions.Contains(ext);
    }

    /// <summary>
    /// Verifica si el tamaño del archivo está dentro del límite permitido.
    /// </summary>
    public bool IsAllowedImageSize(long fileSizeBytes) =>
        fileSizeBytes > 0 && fileSizeBytes <= MaxImageFileSizeBytes;

    /// <summary>
    /// Normaliza una extensión asegurando que empiece por punto y esté en minúsculas.
    /// </summary>
    private static string NormalizeExtension(string input)
    {
        var trimmed = input.Trim();
        var ext = Path.GetExtension(trimmed);
        if (string.IsNullOrWhiteSpace(ext))
            ext = trimmed.StartsWith(".") ? trimmed : $".{trimmed}";
        return ext.ToLowerInvariant();
    }
}
