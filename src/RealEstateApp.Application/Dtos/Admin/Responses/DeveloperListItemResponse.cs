namespace RealEstateApp.Application.Dtos.Admin.Responses;

public sealed record DeveloperListItemResponse(
    string Id,
    string FirstName,
    string LastName,
    string? UserName,
    string? IdentityDocument,
    string? Email,
    bool IsActive
);
