namespace RealEstateApp.Application.ViewModels.Credits;

public class CreditsViewModel
{
    public string ProjectName { get; set; } = "RealEstateApp";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "Plataforma inmobiliaria full-stack con .NET 9 + Tailwind CSS";
    public List<TeamMemberViewModel> Team { get; set; } =
    [
        new TeamMemberViewModel
        {
            Name = "Leonardo Enrique Tavarez Beltran",
            Initials = "LT",
            Role = "Project Master / Esclavizador Master",
            AvatarColor = "bg-amber-500",
            RoleColor = "text-amber-600",
            AccentColor = "bg-amber-500",
            IsMaster = true,
        },
        new TeamMemberViewModel
        {
            Name = "Angel Gonzalez Muñoz",
            Initials = "AM",
            Role = "Lead Developer",
            AvatarColor = "bg-brand-600",
            RoleColor = "text-brand-600",
            AccentColor = "bg-brand-600",
            GitHubUrl = "https://github.com/xNeuNoRo",
        },
        new TeamMemberViewModel
        {
            Name = "Isaias Jose Morillo Ferreras",
            Initials = "IF",
            Role = "Desarrollador de Software",
            AvatarColor = "bg-emerald-500",
            RoleColor = "text-emerald-700",
            AccentColor = "bg-emerald-500",
            GitHubUrl = "https://github.com/IsaiasMorillo",
        },
        new TeamMemberViewModel
        {
            Name = "Engel Orlando Acosta Santos",
            Initials = "AS",
            Role = "Desarrollador de Software",
            AvatarColor = "bg-violet-500",
            RoleColor = "text-purple-700",
            AccentColor = "bg-violet-500",
            GitHubUrl = "https://github.com/notengel",
        },
    ];
}
