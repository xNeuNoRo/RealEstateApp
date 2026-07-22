namespace RealEstateApp.Application.ViewModels.Credits;

public class TeamMemberViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "bg-brand-600";
    public string RoleColor { get; set; } = "text-brand-600";
    public string AccentColor { get; set; } = "bg-brand-600";
    public string? GitHubUrl { get; set; }
    public bool IsMaster { get; set; }
}
