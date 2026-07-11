namespace RealEstateApp.Application.Dtos.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public DateTime Expiration { get; set; }
}
