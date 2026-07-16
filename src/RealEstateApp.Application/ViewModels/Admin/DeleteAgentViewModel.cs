namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class DeleteAgentViewModel
{
    public string AgentId { get; init; } = null!;
    public string AgentName { get; init; } = null!;
    public string? AgentEmail { get; init; }
    public int PropertiesCount { get; init; }
}
