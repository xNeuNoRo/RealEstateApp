using Microsoft.AspNetCore.Mvc;

namespace RealEstateApp.Api.Extensions;

public static class MvcOptionsExtensions
{
    public static void ConfigureModelBindingMessages(this MvcOptions options)
    {
        var provider = options.ModelBindingMessageProvider;

        provider.SetAttemptedValueIsInvalidAccessor(
            (v, f) => $"El valor '{v}' no es válido para el campo '{f}'."
        );

        provider.SetUnknownValueIsInvalidAccessor(
            (f) => $"El valor proporcionado no es válido para el campo '{f}'."
        );

        provider.SetNonPropertyAttemptedValueIsInvalidAccessor(
            (v) => $"El valor '{v}' no es válido."
        );

        provider.SetNonPropertyUnknownValueIsInvalidAccessor(() =>
            "El valor proporcionado no es válido."
        );

        provider.SetValueIsInvalidAccessor((v) => $"El valor '{v}' es inválido.");

        provider.SetValueMustNotBeNullAccessor((f) => $"El valor '{f}' no puede ser nulo.");

        provider.SetMissingRequestBodyRequiredValueAccessor(() =>
            "El cuerpo de la petición no puede estar vacío."
        );

        provider.SetMissingBindRequiredValueAccessor(
            (f) => $"Falta proporcionar un valor para el campo requerido '{f}'."
        );

        provider.SetMissingKeyOrValueAccessor(() => "Se requiere proporcionar un valor.");

        provider.SetNonPropertyValueMustBeANumberAccessor(() =>
            "El valor debe ser un número válido."
        );

        provider.SetValueMustBeANumberAccessor((f) => $"El campo '{f}' debe ser un número válido.");
    }
}
