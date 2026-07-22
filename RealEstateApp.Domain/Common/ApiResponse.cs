using System.Text.Json.Serialization;

namespace RealEstateApp.Domain.Common;

public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool IsSuccess { get; }
    public T? Data { get; }
    public ApiError? Error { get; }

    internal ApiResponse(bool isSuccess, T? data, ApiError? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static ApiResponse<T> Success(T data) => new(true, data, null);

    public static ApiResponse<T> Failure(ApiError error) => new(false, default, error);

    public static ApiResponse<T> Failure(string code, string message, object? details = null) =>
        Failure(new ApiError(code, message, details));
}

public static class ApiResponse
{
    public static ApiResponse<object> Success() => new(true, null, null);

    public static ApiResponse<object> Failure(ApiError error) => new(false, null, error);

    public static ApiResponse<object> Failure(
        string code,
        string message,
        object? details = null
    ) => Failure(new ApiError(code, message, details));
}
