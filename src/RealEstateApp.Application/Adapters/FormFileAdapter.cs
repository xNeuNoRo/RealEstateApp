using Microsoft.AspNetCore.Http;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Adapters;

public sealed class FormFileAdapter : IAppFile
{
    private readonly IFormFile _file;

    public FormFileAdapter(IFormFile file) => _file = file ?? throw new ArgumentNullException(nameof(file));

    public Stream Content => _file.OpenReadStream();
    public string FileName => _file.FileName;
    public string ContentType => _file.ContentType;
    public long Length => _file.Length;
}

public static class FormFileExtensions
{
    public static IEnumerable<IAppFile> ToAppFiles(this IFormFileCollection files) =>
        files.Select(f => new FormFileAdapter(f));

    public static IAppFile? ToAppFile(this IFormFile? file) =>
        file is null ? null : new FormFileAdapter(file);
}
