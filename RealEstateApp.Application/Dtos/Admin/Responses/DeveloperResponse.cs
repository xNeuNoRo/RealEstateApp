namespace RealEstateApp.Application.Dtos.Admin.Responses;

public sealed record DeveloperResponse(
    string Id,
    string FirstName,
    string LastName,
    string? UserName,
    string? IdentityDocument,
    string? Email,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<string> Roles
);
