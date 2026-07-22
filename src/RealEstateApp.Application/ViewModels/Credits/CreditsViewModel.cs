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
            AvatarShadow = "shadow-amber-200 dark:shadow-amber-900/50",
            RoleColor = "text-amber-600 dark:text-amber-400",
            HoverColor = "hover:bg-amber-50 dark:hover:bg-amber-900/20",
            IconColor = "group-hover:text-amber-600",
            IsMaster = true,
        },
        new TeamMemberViewModel
        {
            Name = "Angel Gonzalez Muñoz",
            Initials = "AM",
            Role = "Lead Developer",
            AvatarColor = "bg-brand-600",
            AvatarShadow = "shadow-brand-200 dark:shadow-brand-900/50",
            RoleColor = "text-brand-600 dark:text-brand-400",
            HoverColor = "hover:bg-brand-50 dark:hover:bg-brand-900/20",
            IconColor = "group-hover:text-brand-600",
            GitHubUrl = "https://github.com/xNeuNoRo",
        },
        new TeamMemberViewModel
        {
            Name = "Isaias Jose Morillo Ferreras",
            Initials = "IF",
            Role = "Desarrollador de Software",
            AvatarColor = "bg-emerald-500",
            AvatarShadow = "shadow-emerald-200 dark:shadow-emerald-900/50",
            RoleColor = "text-emerald-600 dark:text-emerald-400",
            HoverColor = "hover:bg-emerald-50 dark:hover:bg-emerald-900/20",
            IconColor = "group-hover:text-emerald-600",
            GitHubUrl = "https://github.com/IsaiasMorillo",
        },
        new TeamMemberViewModel
        {
            Name = "Engel Orlando Acosta Santos",
            Initials = "AS",
            Role = "Desarrollador de Software",
            AvatarColor = "bg-violet-500",
            AvatarShadow = "shadow-violet-200 dark:shadow-violet-900/50",
            RoleColor = "text-violet-600 dark:text-violet-400",
            HoverColor = "hover:bg-violet-50 dark:hover:bg-violet-900/20",
            IconColor = "group-hover:text-violet-600",
            GitHubUrl = "https://github.com/notengel",
        },
    ];
}
