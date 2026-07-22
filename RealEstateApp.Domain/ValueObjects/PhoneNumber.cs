using System.Text.RegularExpressions;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.ValueObjects;

/// <summary>
/// Número de teléfono. Validado: solo dígitos, espacios, +, -, paréntesis.
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex Pattern = new(
        @"^\+?[\d\s\-()]{7,20}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PhoneNumber>(
                Error.Validation("Phone.Empty", "El teléfono no puede estar vacío.")
            );
        var normalized = value.Trim();
        if (!Pattern.IsMatch(normalized))
            return Result.Failure<PhoneNumber>(
                Error.Validation("Phone.Invalid", "El formato del teléfono no es válido.")
            );

        return Result.Success(new PhoneNumber(normalized));
    }

    public static PhoneNumber Unsafe(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
