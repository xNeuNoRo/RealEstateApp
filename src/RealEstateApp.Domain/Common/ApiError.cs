namespace RealEstateApp.Domain.Common;

public class ApiError
{
    public string Code { get; }
    public string Message { get; }
    public object? Details { get; }

    public ApiError(string code, string message, object? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }
}
