namespace RealEstateApp.Application.Dtos.Auth.Responses;

/// <summary>
/// Datos del usuario retornados tras login o registro exitoso.
/// </summary>
public sealed class AuthResponse
{
    public string UserId { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    public bool RequiresActivation { get; init; }
    public string? Message { get; init; }
}
