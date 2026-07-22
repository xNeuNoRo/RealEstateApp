namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class AdminDashboardViewModel : BaseViewModel
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
