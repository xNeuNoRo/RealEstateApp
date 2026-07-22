namespace RealEstateApp.Application.Dtos.Admin.Requests;

public sealed record ChangeAgentStatusRequest(string AgentId, bool Status);
