using System.Text.RegularExpressions;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.ValueObjects;

/// <summary>
/// Código único de 6 dígitos para cada propiedad. Valida formato.
/// </summary>
public sealed class PropertyCode : ValueObject
{
    public string Value { get; }

    private PropertyCode(string value) => Value = value;

    public static Result<PropertyCode> Create(string value)
    {
        if (
            string.IsNullOrWhiteSpace(value)
            || value.Length != 6
            || !Regex.IsMatch(value, "^[0-9]{6}$")
        )
            return Result.Failure<PropertyCode>(
                Error.Validation(
                    "PropertyCode.Invalid",
                    "El código debe tener exactamente 6 dígitos."
                )
            );

        return Result.Success(new PropertyCode(value));
    }

    /// <summary>Omite validación; solo para valores ya confiables desde la BD.</summary>
    public static PropertyCode Unsafe(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
