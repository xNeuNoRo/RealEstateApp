namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class AgentListItemViewModel
{
    public string Id { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? Email { get; init; }
    public string? UserName { get; init; }
    public int PropertiesCount { get; init; }
    public bool IsActive { get; init; }
    public string FullName => $"{FirstName} {LastName}";
    public string StatusLabel => IsActive ? "Activo" : "Inactivo";
}
