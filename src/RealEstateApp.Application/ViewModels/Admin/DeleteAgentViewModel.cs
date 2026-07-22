namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class DeleteAgentViewModel : BaseViewModel
{
    public string AgentId { get; init; } = null!;
    public string AgentName { get; init; } = null!;
    public string? AgentEmail { get; init; }
    public int PropertiesCount { get; init; }
}
