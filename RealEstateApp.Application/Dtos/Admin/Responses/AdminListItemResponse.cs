namespace RealEstateApp.Application.Dtos.Admin.Responses;

public sealed record AdminListItemResponse(
    string Id,
    string FirstName,
    string LastName,
    string? UserName,
    string? IdentityDocument,
    string? Email,
    bool IsActive
);
