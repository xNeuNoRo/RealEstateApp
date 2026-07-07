using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.ValueObjects;

public sealed class IdentityDocument : ValueObject
{
    public string Value { get; }

    private IdentityDocument(string value) => Value = value;

    public static Result<IdentityDocument> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<IdentityDocument>(
                Error.Validation("IdentityDocument.Empty", "La cédula no puede estar vacía.")
            );
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 11 || !long.TryParse(digits, out _))
            return Result.Failure<IdentityDocument>(
                Error.Validation(
                    "IdentityDocument.Length",
                    "La cédula debe tener exactamente 11 dígitos."
                )
            );
        if (!IsValidCheckDigit(digits))
            return Result.Failure<IdentityDocument>(
                Error.Validation(
                    "IdentityDocument.Checksum",
                    "El dígito verificador de la cédula no es válido."
                )
            );

        return Result.Success(new IdentityDocument(digits));
    }

    public static IdentityDocument Unsafe(string value) => new(value);

    private static bool IsValidCheckDigit(string digits)
    {
        int sum = 0;
        for (int i = 0; i < 10; i++)
        {
            int d = digits[i] - '0';
            int w = (i % 2 == 0) ? 1 : 2;
            int prod = d * w;
            sum += prod >= 10 ? prod - 9 : prod;
        }
        int check = (10 - (sum % 10)) % 10;
        return check == (digits[10] - '0');
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
