namespace RealEstateApp.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se viola un invariante o regla de negocio dentro del dominio.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }
    public IEnumerable<string> Errors { get; }

    public DomainException(string code, string message)
        : base(message)
    {
        Code = code;
        Errors = Enumerable.Empty<string>();
    }

    public DomainException(string code, string message, IEnumerable<string> errors)
        : base(message)
    {
        Code = code;
        Errors = errors;
    }

    public DomainException(string code, string message, Exception inner)
        : base(message, inner)
    {
        Code = code;
        Errors = Enumerable.Empty<string>();
    }
}
