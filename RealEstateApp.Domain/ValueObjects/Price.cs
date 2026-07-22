using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.ValueObjects;

/// <summary>
/// Monto monetario en una moneda fija. Inmutable. Estrictamente positivo.
/// </summary>
public sealed class Price : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Price> Create(decimal amount, string currency = "DOP")
    {
        if (amount <= 0)
            return Result.Failure<Price>(
                Error.Validation("Price.Invalid", "El precio debe ser mayor que cero.")
            );
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            return Result.Failure<Price>(
                Error.Validation(
                    "Price.InvalidCurrency",
                    "La moneda debe ser un código ISO de 3 letras."
                )
            );

        return Result.Success(new Price(amount, currency.ToUpperInvariant()));
    }

    /// <summary>Omite validación; solo llamar con valores de proyección/infra confiables.</summary>
    public static Price Unsafe(decimal amount, string currency = "DOP") => new(amount, currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
