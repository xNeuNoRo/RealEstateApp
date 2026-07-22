using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Api.Controllers.Base;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult Success<T>(T data) => Ok(ApiResponse<T>.Success(data));

    protected IActionResult Success() => Ok(ApiResponse.Success());

    protected IActionResult CreatedSuccess<T>(string actionName, object routeValues, T data) =>
        CreatedAtAction(actionName, routeValues, ApiResponse<T>.Success(data));

    protected IActionResult SuccessOrNotFound<T>(
        T? data,
        string errorCode = ErrorCodes.NotFound,
        string message = "El recurso solicitado no fue encontrado."
    )
        where T : class =>
        data is null ? NotFound(ApiResponse<object>.Failure(errorCode, message)) : Success(data);

    protected IActionResult SuccessOrNotFound(
        bool condition,
        string errorCode = ErrorCodes.NotFound,
        string message = "El recurso solicitado no fue encontrado."
    ) => condition ? Success() : NotFound(ApiResponse<object>.Failure(errorCode, message));

    protected IActionResult FailureResponse(string code, string message, int statusCode = 400) =>
        StatusCode(statusCode, ApiResponse<object>.Failure(code, message));

    protected IActionResult ConflictResponse(string message, string code = ErrorCodes.Conflict) =>
        FailureResponse(code, message, 409);

    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Success(result.GetValue());

        return MapError(result.GetError());
    }

    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
            return Success();

        return MapError(result.GetError());
    }

    private IActionResult MapError(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => FailureResponse(error.Code, error.Message, 400),
            ErrorType.NotFound => FailureResponse(error.Code, error.Message, 404),
            ErrorType.Conflict => FailureResponse(error.Code, error.Message, 409),
            ErrorType.Unauthorized => FailureResponse(error.Code, error.Message, 401),
            ErrorType.Forbidden => FailureResponse(error.Code, error.Message, 403),
            ErrorType.Failure => FailureResponse(error.Code, error.Message, 500),
            _ => FailureResponse(error.Code, error.Message, 500),
        };
}
