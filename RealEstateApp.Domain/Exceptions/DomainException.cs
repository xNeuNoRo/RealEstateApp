namespace RealEstateApp.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se viola un invariante o regla de negocio dentro del dominio.
/// </summary>
public class DomainException : AppException
{
    public IEnumerable<string> Errors { get; }

    public DomainException(string code, string message)
        : base(400, code, message)
    {
        Errors = Enumerable.Empty<string>();
    }

    public DomainException(string code, string message, IEnumerable<string> errors)
        : base(400, code, message)
    {
        Errors = errors;
    }

    public DomainException(string code, string message, Exception inner)
        : base(400, code, message, inner)
    {
        Errors = Enumerable.Empty<string>();
    }
}
