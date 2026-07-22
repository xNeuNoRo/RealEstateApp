namespace RealEstateApp.Application.Dtos.Auth;

public class EditUserDto
{
    public string Id { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Role { get; set; } = null!;
}
