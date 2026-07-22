namespace RealEstateApp.Domain.Common;

/// <summary>
/// Proveedor estático de tiempo para el dominio. Por defecto usa TimeProvider.System.
/// </summary>
public static class DomainTime
{
    private static TimeProvider _provider = TimeProvider.System;

    public static TimeProvider Provider
    {
        get => _provider;
        set => _provider = value ?? TimeProvider.System;
    }

    /// <summary>Obtiene la fecha/hora actual en UTC.</summary>
    public static DateTimeOffset UtcNow => _provider.GetUtcNow();
}
