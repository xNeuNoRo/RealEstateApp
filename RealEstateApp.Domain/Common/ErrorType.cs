namespace RealEstateApp.Domain.Common;

/// <summary>
/// Categorías de error usadas por Result. Determinan el mapeo a códigos HTTP y mensajes de UI.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Failure,
}
