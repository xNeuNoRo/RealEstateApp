namespace RealEstateApp.Application.Dtos.Admin.Responses;

public sealed record AdminDashboardResponse(
    int AvailableProperties,
    int SoldProperties,
    int ActiveAgents,
    int InactiveAgents,
    int ActiveClients,
    int InactiveClients,
    int ActiveDevelopers,
    int InactiveDevelopers
);
