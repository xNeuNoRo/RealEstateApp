namespace RealEstateApp.Application.Dtos.Auth;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string? IdentityDocument { get; set; }
    public bool Active { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}
