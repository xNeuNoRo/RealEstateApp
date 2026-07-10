namespace RealEstateApp.Domain.Settings;

/// <summary>
/// Configuración de JWT para la autenticación de la API.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int DurationInMinutes { get; set; }
}
