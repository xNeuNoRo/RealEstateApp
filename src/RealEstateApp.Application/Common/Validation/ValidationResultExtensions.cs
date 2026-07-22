using FluentValidation.Results;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Common.Validation;

/// <summary>
/// Extensiones para convertir <see cref="ValidationResult"/> de FluentValidation
/// en el patrón <see cref="Result"/>/<see cref="Error"/> del dominio.
/// </summary>
public static class ValidationResultExtensions
{
    private const string CombinedValidationCode = "Validation";
    private const string Separator = "; ";

    /// <summary>
    /// Convierte un <see cref="ValidationResult"/> fallido en un <see cref="Error"/> de validación
    /// combinando todos los mensajes de error en uno solo.
    /// </summary>
    public static Error ToError(this ValidationResult result, string code = CombinedValidationCode)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.Errors.Count == 0)
        {
            return Error.Validation(
                CombinedValidationCode,
                "Falló la validación sin errores detallados."
            );
        }

        var message = string.Join(
            Separator,
            result.Errors.Select(failure => failure.ErrorMessage)
        );

        return Error.Validation(code, message);
    }

    /// <summary>
    /// Convierte un <see cref="ValidationResult"/> fallido en un <see cref="Result"/> fallido.
    /// Atajo para <c>Result.Failure(result.ToError())</c>.
    /// </summary>
    public static Result ToResult(
        this ValidationResult result,
        string code = CombinedValidationCode
    )
    {
        ArgumentNullException.ThrowIfNull(result);
        return Result.Failure(result.ToError(code));
    }

    /// <summary>
    /// Convierte un <see cref="ValidationResult"/> fallido en un <see cref="Result{T}"/> fallido.
    /// Atajo para <c>Result.Failure<T>(result.ToError())</c>.
    /// </summary>
    public static Result<T> ToResult<T>(
        this ValidationResult result,
        string code = CombinedValidationCode
    )
    {
        ArgumentNullException.ThrowIfNull(result);
        return Result<T>.Failure(result.ToError(code));
    }
}
