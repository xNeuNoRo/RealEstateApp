using System.Text.RegularExpressions;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.ValueObjects;

/// <summary>
/// Dirección de correo electrónico. Validación de formato vía regex estilo RFC; sin verificación de red.
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex Pattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(
                Error.Validation("Email.Empty", "El email no puede estar vacío.")
            );
        var normalized = value.Trim().ToLowerInvariant();
        if (!Pattern.IsMatch(normalized))
            return Result.Failure<Email>(
                Error.Validation("Email.Invalid", "El formato del email no es válido.")
            );

        return Result.Success(new Email(normalized));
    }

    public static Email Unsafe(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
