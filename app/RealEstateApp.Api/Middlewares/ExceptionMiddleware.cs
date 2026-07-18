using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using RealEstateApp.Domain.Common;
using DomainAppException = RealEstateApp.Domain.Exceptions.AppException;
using DomainDomainException = RealEstateApp.Domain.Exceptions.DomainException;
using DomainValidationException = RealEstateApp.Domain.Exceptions.ValidationException;

namespace RealEstateApp.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private static Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ILogger logger
    )
    {
        if (context.Response.HasStarted)
        {
            logger.LogWarning(
                "No se puede manejar la excepción porque la respuesta ya ha comenzado a enviarse al cliente."
            );
            return Task.CompletedTask;
        }

        context.Response.ContentType = "application/json";

        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "Ha ocurrido un error inesperado en el servidor.";
        var errorCode = ErrorCodes.InternalServerError;
        IReadOnlyDictionary<string, string[]>? validationErrors = null;

        if (exception is DomainAppException appEx)
        {
            statusCode = appEx.StatusCode;
            message = appEx.Message;
            errorCode = appEx.Code;

            logger.LogWarning(
                "Excepción de aplicación capturada: {Message} (Code: {Code}, Status: {Status})",
                message,
                errorCode,
                statusCode
            );
        }
        else if (exception is DomainDomainException domEx)
        {
            statusCode = domEx.StatusCode;
            message = domEx.Message;
            errorCode = string.IsNullOrWhiteSpace(domEx.Code)
                ? ErrorCodes.ValidationError
                : domEx.Code;

            logger.LogWarning(
                "Violación de regla de dominio: {Message} (Code: {Code})",
                message,
                errorCode
            );
        }
        else if (exception is DomainValidationException valEx)
        {
            statusCode = valEx.StatusCode;
            message = valEx.Message;
            errorCode = valEx.Code;
            validationErrors = valEx.Errors;
        }
        else if (exception is ValidationException fluentValEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            message = "Uno o más campos tienen errores de validación.";
            errorCode = ErrorCodes.ValidationError;
            validationErrors = fluentValEx
                .Errors.GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }
        else
        {
            logger.LogError(
                exception,
                "Ha ocurrido un error crítico no controlado en el servidor."
            );
        }

        context.Response.StatusCode = statusCode;

        var error = validationErrors is not null
            ? new ApiError(errorCode, message, validationErrors)
            : new ApiError(errorCode, message);

        var response = ApiResponse.Failure(error);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
        };

        return context.Response.WriteAsJsonAsync(response, options);
    }
}
