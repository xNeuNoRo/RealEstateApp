using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Interfaces;

/// <summary>
/// Servicio de almacenamiento de archivos con validación de integridad y seguridad.
/// </summary>
public interface IFileService
{
    string GetAbsolutePath(string relativePath);
    Task<string> UploadFileAsync(IAppFile file, string folderName);
    Task<string> UploadTempFileAsync(IAppFile file);
    bool IsImageValid(IAppFile file);
    void DeleteFile(string filePath);
    Task DeleteFileAsync(string filePath);
}
