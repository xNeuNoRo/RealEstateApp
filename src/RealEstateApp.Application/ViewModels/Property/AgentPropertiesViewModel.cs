namespace RealEstateApp.Application.ViewModels.Property;

public sealed class AgentPropertiesViewModel
{
    public string AgentId { get; init; } = null!;
    public string AgentName { get; init; } = null!;
    public string? AgentEmail { get; init; }
    public string? AgentPhone { get; init; }
    public string? AgentProfileImage { get; init; }
    public IReadOnlyList<PropertyListItemViewModel> Properties { get; init; } = [];
}
