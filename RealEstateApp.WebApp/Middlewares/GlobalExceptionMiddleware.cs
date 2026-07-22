using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Exceptions;

namespace RealEstateApp.WebApp.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITempDataDictionaryFactory tempDataFactory)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el flujo de la aplicación.");
            await HandleExceptionAsync(context, ex, tempDataFactory);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ITempDataDictionaryFactory tempDataFactory
    )
    {
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "Ocurrió un error al procesar la solicitud. Inténtelo nuevamente.";
        string errorCode = ErrorCodes.InternalServerError;

        if (exception is ValidationException valEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            var messages = valEx.Errors?.SelectMany(kv => kv.Value);
            message = messages != null && messages.Any()
                ? string.Join(" ", messages)
                : valEx.Message;
            errorCode = ErrorCodes.ValidationError;
        }
        else if (exception is DomainException domEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            message = domEx.Message;
            errorCode = domEx.Code;
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = (int)HttpStatusCode.Unauthorized;
            message = "No tiene permisos para realizar esta acción.";
            errorCode = ErrorCodes.Unauthorized;
        }
        else if (exception is EntityNotFoundException)
        {
            statusCode = (int)HttpStatusCode.NotFound;
            message = exception.Message;
            errorCode = ErrorCodes.NotFound;
        }
        else if (exception is KeyNotFoundException)
        {
            statusCode = (int)HttpStatusCode.NotFound;
            message = "El recurso solicitado no fue encontrado.";
            errorCode = ErrorCodes.NotFound;
        }

        bool isAjax =
            context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isAjax)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new { Message = message, Code = errorCode };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        else
        {
            var tempData = tempDataFactory.GetTempData(context);
            tempData["ErrorMessage"] = message;
            tempData["ErrorCode"] = errorCode;
            tempData.Save();

            var referer = context.Request.Headers["Referer"].ToString();

            if (
                !string.IsNullOrEmpty(referer)
                && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri)
                && string.Equals(
                    refererUri.Host,
                    context.Request.Host.Host,
                    StringComparison.OrdinalIgnoreCase
                )
                && !referer.Contains("/Home/Error")
                && !referer.Contains("/Auth/Login")
                && !referer.Contains("/Auth/Register")
            )
            {
                context.Response.Redirect(referer);
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
