namespace RealEstateApp.Domain.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }

    protected AppException(int statusCode, string code, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }

    protected AppException(int statusCode, string code, string message, Exception inner)
        : base(message, inner)
    {
        StatusCode = statusCode;
        Code = code;
    }
}
