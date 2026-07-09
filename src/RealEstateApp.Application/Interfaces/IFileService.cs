using Microsoft.AspNetCore.Http;

namespace RealEstateApp.Application.Interfaces;

/// <summary>
/// Servicio de almacenamiento de archivos con validación de integridad y seguridad.
/// </summary>
public interface IFileService
{
    string GetAbsolutePath(string relativePath);
    Task<string> UploadFileAsync(IFormFile file, string folderName);
    Task<string> UploadTempFileAsync(IFormFile file);
    bool IsImageValid(IFormFile file);
    void DeleteFile(string filePath);
    Task DeleteFileAsync(string filePath);
}
