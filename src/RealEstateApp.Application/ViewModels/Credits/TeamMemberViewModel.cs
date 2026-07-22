namespace RealEstateApp.Application.ViewModels.Credits;

public class TeamMemberViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "bg-brand-600";
    public string AvatarShadow { get; set; } = "shadow-brand-200 dark:shadow-brand-900/50";
    public string RoleColor { get; set; } = "text-brand-600 dark:text-brand-400";
    public string HoverColor { get; set; } = "hover:bg-brand-50 dark:hover:bg-brand-900/20";
    public string IconColor { get; set; } = "group-hover:text-brand-600";
    public string? GitHubUrl { get; set; }
    public bool IsMaster { get; set; }
}
