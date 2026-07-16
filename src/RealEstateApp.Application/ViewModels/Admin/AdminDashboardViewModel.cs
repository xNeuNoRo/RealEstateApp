namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class AdminDashboardViewModel
{
    public int AvailableProperties { get; init; }
    public int SoldProperties { get; init; }
    public int ActiveAgents { get; init; }
    public int InactiveAgents { get; init; }
    public int ActiveClients { get; init; }
    public int InactiveClients { get; init; }
    public int ActiveDevelopers { get; init; }
    public int InactiveDevelopers { get; init; }
}
