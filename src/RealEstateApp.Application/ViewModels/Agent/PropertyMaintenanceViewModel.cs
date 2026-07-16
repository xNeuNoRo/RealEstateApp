namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class PropertyMaintenanceViewModel
{
    public IReadOnlyList<AgentPropertyListItemViewModel> Properties { get; init; } = [];
}
