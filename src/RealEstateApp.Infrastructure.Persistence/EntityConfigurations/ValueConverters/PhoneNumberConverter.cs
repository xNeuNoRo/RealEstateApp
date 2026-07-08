using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations.ValueConverters;

/// <summary>
/// Convierte <see cref="PhoneNumber"/> a string en BD y viceversa.
/// Usa <c>Unsafe</c> al leer porque el dato proviene de la BD y ya fue validado al insertar.
/// </summary>
public sealed class PhoneNumberConverter : ValueConverter<PhoneNumber, string>
{
    public PhoneNumberConverter()
        : base(v => v.Value, v => PhoneNumber.Unsafe(v)) { }
}
