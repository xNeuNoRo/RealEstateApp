namespace RealEstateApp.Application.Dtos.Admin.Responses;

public sealed record AgentListItemResponse(
    string Id,
    string FirstName,
    string LastName,
    string? Email,
    string? UserName,
    int PropertiesCount,
    bool IsActive
);
