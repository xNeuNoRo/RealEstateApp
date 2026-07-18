namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class AgentHomeViewModel : BaseViewModel
{
    public IReadOnlyList<AgentPropertyListItemViewModel> AvailableProperties { get; init; } = [];
    public IReadOnlyList<AgentPropertyListItemViewModel> SoldProperties { get; init; } = [];
    public int TotalProperties => AvailableProperties.Count + SoldProperties.Count;
}
