using System.Globalization;

namespace RealEstateApp.WebApp.Helpers;

public static class PriceHelper
{
    private static readonly CultureInfo DominicanCulture = new("es-DO");

    public static string Format(decimal amount, string? currency = "DOP")
    {
        var culture = currency?.ToUpperInvariant() switch
        {
            "USD" => new CultureInfo("en-US"),
            "EUR" => new CultureInfo("es-ES"),
            _ => DominicanCulture,
        };

        var formatted = amount.ToString("N2", culture);

        return currency?.ToUpperInvariant() switch
        {
            "DOP" => $"RD${formatted}",
            "USD" => $"US${formatted}",
            "EUR" => $"€{formatted}",
            _ => $"{formatted} {currency}",
        };
    }

    public static string FormatCompact(decimal amount, string? currency = "DOP")
    {
        var abs = Math.Abs(amount);
        string value;
        string suffix;

        if (abs >= 1_000_000_000)
        {
            value = (amount / 1_000_000_000).ToString("0.#", DominicanCulture);
            suffix = "B";
        }
        else if (abs >= 1_000_000)
        {
            value = (amount / 1_000_000).ToString("0.#", DominicanCulture);
            suffix = "M";
        }
        else if (abs >= 1_000)
        {
            value = (amount / 1_000).ToString("0.#", DominicanCulture);
            suffix = "K";
        }
        else
        {
            return Format(amount, currency);
        }

        var prefix = currency?.ToUpperInvariant() switch
        {
            "DOP" => "RD$",
            "USD" => "US$",
            "EUR" => "€",
            _ => "",
        };

        return $"{prefix}{value}{suffix}";
    }

    public static string FormatArea(decimal sizeM2, string? unit = "m²")
    {
        return $"{sizeM2:N2} {unit}";
    }
}
