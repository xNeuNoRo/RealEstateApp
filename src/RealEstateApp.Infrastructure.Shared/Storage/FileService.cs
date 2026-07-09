using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Domain.Settings;

namespace RealEstateApp.Infrastructure.Shared.Storage;

/// <summary>
/// Servicio de almacenamiento de archivos con validación de integridad y seguridad.
/// </summary>
public class FileService : IFileService
{
    private readonly FileSettings _settings;
    private readonly ILogger<FileService> _logger;

    public FileService(IOptions<FileSettings> options, ILogger<FileService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Resuelve una ruta relativa a absoluta verificando que no salga del directorio base.
    /// </summary>
    public string GetAbsolutePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        string cleanRelativePath = relativePath.TrimStart('/', '\\');
        string baseAbsolutePath = Path.GetFullPath(_settings.BasePath);
        string combinedPath = Path.GetFullPath(Path.Combine(baseAbsolutePath, cleanRelativePath));

        if (!combinedPath.StartsWith(baseAbsolutePath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogCritical(
                "Intento de acceso ilegal fuera del directorio base (Path Traversal): {Path}",
                combinedPath
            );
            return string.Empty;
        }

        return combinedPath;
    }

    /// <summary>
    /// Guarda un archivo en la subcarpeta indicada y retorna la ruta relativa pública.
    /// </summary>
    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        try
        {
            string sanitizedFolder = string.Join(
                    "_",
                    folderName.Split(Path.GetInvalidFileNameChars())
                )
                .Replace("..", "")
                .Trim('/', '\\');

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            string uploadRoot = Path.Combine(
                _settings.UrlPrefix.TrimStart('/', '\\'),
                sanitizedFolder
            );
            string absoluteFolderPath = GetAbsolutePath(uploadRoot);

            if (string.IsNullOrEmpty(absoluteFolderPath))
                throw new InvalidOperationException("La ruta de carga no es valida o es insegura.");

            string relativePath = $"/{_settings.UrlPrefix.Trim('/')}/{sanitizedFolder}/{fileName}";

            if (!Directory.Exists(absoluteFolderPath))
                Directory.CreateDirectory(absoluteFolderPath);

            string absoluteFilePath = Path.Combine(absoluteFolderPath, fileName);
            using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Archivo guardado exitosamente: {Path}", relativePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error critico al intentar guardar archivo.");
            throw;
        }
    }

    /// <summary>
    /// Guarda un archivo temporal y retorna la ruta relativa.
    /// </summary>
    public async Task<string> UploadTempFileAsync(IFormFile file)
    {
        try
        {
            string fileName = $"temp_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string absoluteTempPath = GetAbsolutePath(FileConstants.TempFolder);

            if (string.IsNullOrEmpty(absoluteTempPath))
                throw new InvalidOperationException("El directorio temporal no es seguro.");

            if (!Directory.Exists(absoluteTempPath))
                Directory.CreateDirectory(absoluteTempPath);

            string fullPath = Path.Combine(absoluteTempPath, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"{FileConstants.TempFolder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo temporal.");
            throw;
        }
    }

    /// <summary>
    /// Valida que un archivo sea una imagen permitida por tamaño, extensión, MIME y magic bytes.
    /// </summary>
    public bool IsImageValid(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (!_settings.IsAllowedImageSize(file.Length))
        {
            _logger.LogWarning(
                "Intento de carga de imagen excediendo tamano: {Size} bytes",
                file.Length
            );
            return false;
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.IsAllowedImageExtension(extension))
        {
            _logger.LogWarning("Extension de archivo no permitida: {Extension}", extension);
            return false;
        }

        string mimeType = file.ContentType.ToLowerInvariant();
        if (!_settings.AllowedMimeTypes.Contains(mimeType))
        {
            _logger.LogWarning("MIME type no permitido: {MimeType}", mimeType);
            return false;
        }

        if (!HasValidMagicBytes(file, extension))
        {
            _logger.LogWarning("Magic bytes invalidos para extension: {Extension}", extension);
            return false;
        }

        return true;
    }

    private bool HasValidMagicBytes(IFormFile file, string extension)
    {
        if (!_settings.ImageMagicBytes.TryGetValue(extension, out var expected) || expected is null)
            return false;

        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[expected.Length];
            int read = stream.Read(buffer, 0, expected.Length);

            if (read < expected.Length)
                return false;

            for (int i = 0; i < expected.Length; i++)
            {
                if (buffer[i] != expected[i])
                    return false;
            }

            // Validacion adicional para WEBP: bytes 8-11 deben ser "WEBP"
            if (extension == ".webp")
            {
                stream.Seek(8, SeekOrigin.Begin);
                var webpHeader = new byte[4];
                int webpRead = stream.Read(webpHeader, 0, 4);
                return webpRead == 4
                    && webpHeader[0] == 0x57 // W
                    && webpHeader[1] == 0x45 // E
                    && webpHeader[2] == 0x42 // B
                    && webpHeader[3] == 0x50; // P
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Elimina un archivo físico por ruta relativa (síncrono).
    /// </summary>
    public void DeleteFile(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string absolutePath = GetAbsolutePath(filePath);

            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
                _logger.LogInformation("Archivo eliminado del servidor: {Path}", absolutePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar el archivo fisico: {Path}.", filePath);
        }
    }

    /// <summary>
    /// Elimina un archivo físico por ruta relativa (asíncrono).
    /// </summary>
    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string absolutePath = GetAbsolutePath(filePath);

            if (File.Exists(absolutePath))
            {
                await Task.Run(() => File.Delete(absolutePath));
                _logger.LogInformation(
                    "Archivo eliminado del servidor (async): {Path}",
                    absolutePath
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar el archivo fisico: {Path}.", filePath);
        }
    }
}
