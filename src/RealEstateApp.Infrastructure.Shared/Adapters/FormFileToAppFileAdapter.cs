using Microsoft.AspNetCore.Http;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Models;

namespace RealEstateApp.Infrastructure.Shared.Adapters;

public static class FormFileToAppFileAdapter
{
    public static async Task<IAppFile> AdaptAsync(IFormFile formFile)
    {
        // Iniciamos un stream en memoria
        var ms = new MemoryStream();
        // Copiamos el contenido del IFormFile al MemoryStream
        await formFile.CopyToAsync(ms);
        // Reiniciamos la posición del stream al inicio para que pueda ser leído posteriormente
        ms.Position = 0;
        return new AppFile(ms, formFile.FileName, formFile.ContentType, formFile.Length);
    }
}
