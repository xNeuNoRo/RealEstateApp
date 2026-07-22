using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Models;

public sealed class AppFile : IAppFile
{
    public Stream Content { get; }
    public string FileName { get; }
    public string ContentType { get; }
    public long Length { get; }

    public AppFile(Stream content, string fileName, string contentType, long length)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        Length = length;
    }
}
