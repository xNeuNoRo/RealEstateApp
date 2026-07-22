namespace RealEstateApp.Application.Interfaces.Services;

public interface IAppFile
{
    Stream Content { get; }
    string FileName { get; }
    string ContentType { get; }
    long Length { get; }
}
