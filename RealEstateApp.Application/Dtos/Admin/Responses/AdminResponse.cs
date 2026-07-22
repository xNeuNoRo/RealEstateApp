namespace RealEstateApp.Application.Dtos.Admin.Responses;

public sealed record AdminResponse(
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
