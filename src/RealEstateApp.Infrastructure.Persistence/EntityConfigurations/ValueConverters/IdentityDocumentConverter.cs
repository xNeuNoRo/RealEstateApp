using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations.ValueConverters;

/// <summary>
/// Convierte <see cref="IdentityDocument"/> a string de 11 dígitos en BD y viceversa.
/// Usa <c>Unsafe</c> al leer porque el dato proviene de la BD y ya fue validado al insertar.
/// </summary>
public sealed class IdentityDocumentConverter : ValueConverter<IdentityDocument, string>
{
    public IdentityDocumentConverter()
        : base(v => v.Value, v => IdentityDocument.Unsafe(v)) { }
}
