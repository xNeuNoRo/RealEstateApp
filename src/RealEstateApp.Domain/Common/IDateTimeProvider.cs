namespace RealEstateApp.Domain.Common;

/// <summary>
/// Abstracción del tiempo UTC actual para testabilidad.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
