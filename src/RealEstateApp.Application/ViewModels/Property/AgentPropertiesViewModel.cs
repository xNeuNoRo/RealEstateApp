namespace RealEstateApp.Application.ViewModels.Property;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class AgentPropertiesViewModel : BaseViewModel
{
    public string AgentId { get; init; } = null!;
    public string AgentName { get; init; } = null!;
    public string? AgentEmail { get; init; }
    public string? AgentPhone { get; init; }
    public string? AgentProfileImage { get; init; }
    public IReadOnlyList<PropertyListItemViewModel> Properties { get; init; } = [];
}
