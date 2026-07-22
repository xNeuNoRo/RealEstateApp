namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;

public sealed class PropertyMaintenanceViewModel : BaseViewModel
{
    public PagedResult<AgentPropertyListItemViewModel> Properties { get; init; } =
        new([], 0, 1, 12);
}
