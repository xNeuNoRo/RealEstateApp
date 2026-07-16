namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class AgentHomeViewModel
{
    public IReadOnlyList<AgentPropertyListItemViewModel> AvailableProperties { get; init; } = [];
    public IReadOnlyList<AgentPropertyListItemViewModel> SoldProperties { get; init; } = [];
    public int TotalProperties => AvailableProperties.Count + SoldProperties.Count;
}
