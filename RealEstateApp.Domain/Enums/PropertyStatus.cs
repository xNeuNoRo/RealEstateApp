namespace RealEstateApp.Domain.Enums;

/// <summary>
/// Estado del ciclo de vida de una propiedad. Controla visibilidad en listados y aceptación de ofertas.
/// </summary>
public enum PropertyStatus
{
    Available = 1,
    Sold = 2,
}
