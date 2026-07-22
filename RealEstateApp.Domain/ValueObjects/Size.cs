using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.ValueObjects;

/// <summary>
/// Área superficial de una propiedad. Unidad por defecto m². Estrictamente positivo.
/// </summary>
public sealed class Size : ValueObject
{
    public decimal Area { get; }
    public string Unit { get; }

    private Size(decimal area, string unit)
    {
        Area = area;
        Unit = unit;
    }

    public static Result<Size> Create(decimal area, string unit = "m²")
    {
        if (area <= 0)
            return Result.Failure<Size>(
                Error.Validation("Size.Invalid", "El tamaño debe ser mayor que cero.")
            );
        if (string.IsNullOrWhiteSpace(unit))
            return Result.Failure<Size>(
                Error.Validation("Size.InvalidUnit", "La unidad de medida no puede estar vacía.")
            );

        return Result.Success(new Size(area, unit));
    }

    public static Size Unsafe(decimal area, string unit = "m²") => new(area, unit);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Area;
        yield return Unit;
    }

    public override string ToString() => $"{Area} {Unit}";
}
