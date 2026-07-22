namespace RealEstateApp.Domain.Enums;

/// <summary>
/// Roles de la aplicación. Se seedean en Identity y se usan como políticas de autorización.
/// </summary>
public enum Roles
{
    Client = 1,
    Agent = 2,
    Admin = 3,
    Developer = 4,
}
