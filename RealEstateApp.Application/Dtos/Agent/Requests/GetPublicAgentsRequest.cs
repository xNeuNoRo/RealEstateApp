namespace RealEstateApp.Application.Dtos.Agent.Requests;

public sealed record GetPublicAgentsRequest(
    string? SearchTerm = null,
    string? AgentId = null,
    int Page = 1,
    int PageSize = 12
);
