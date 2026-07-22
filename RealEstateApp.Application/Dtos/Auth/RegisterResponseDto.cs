namespace RealEstateApp.Application.Dtos.Auth;

public class RegisterResponseDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public bool Active { get; set; }
}
