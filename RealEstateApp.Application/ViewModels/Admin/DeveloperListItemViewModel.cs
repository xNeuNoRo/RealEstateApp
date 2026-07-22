namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class DeveloperListItemViewModel : BaseViewModel
{
    public string Id { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? UserName { get; init; }
    public string? IdentityDocument { get; init; }
    public string? Email { get; init; }
    public bool IsActive { get; init; }
    public string FullName => $"{FirstName} {LastName}";
    public string StatusLabel => IsActive ? "Activo" : "Inactivo";
}
