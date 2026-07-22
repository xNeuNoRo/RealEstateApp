using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Exceptions;

public class ValidationException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(400, ErrorCodes.ValidationError, message)
    {
        Errors = errors;
    }
}
