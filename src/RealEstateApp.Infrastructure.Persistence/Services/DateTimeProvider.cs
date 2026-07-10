using RealEstateApp.Domain.Common;

namespace RealEstateApp.Infrastructure.Persistence.Services;

/// <summary>
/// Proveedor de tiempo real. Siempre retorna DateTimeOffset.UtcNow.
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
