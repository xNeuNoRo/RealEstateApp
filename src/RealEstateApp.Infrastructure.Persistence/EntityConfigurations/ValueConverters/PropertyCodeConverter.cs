using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations.ValueConverters;

/// <summary>
/// Convierte <see cref="PropertyCode"/> a su string de 6 dígitos en BD y viceversa.
/// Usa <c>Unsafe</c> al leer porque el dato proviene de la BD y ya fue validado al insertar.
/// </summary>
public sealed class PropertyCodeConverter : ValueConverter<PropertyCode, string>
{
    public PropertyCodeConverter()
        : base(v => v.Value, v => PropertyCode.Unsafe(v)) { }
}
