namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class PropertyMaintenanceViewModel : BaseViewModel
{
    public IReadOnlyList<AgentPropertyListItemViewModel> Properties { get; init; } = [];
}
