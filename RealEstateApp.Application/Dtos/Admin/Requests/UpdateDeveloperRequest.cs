namespace RealEstateApp.Application.Dtos.Admin.Requests;

public sealed record UpdateDeveloperRequest(
    string DeveloperId,
    string FirstName,
    string LastName,
    string IdentityDocument,
    string Email,
    string UserName,
    string? NewPassword = null,
    string? ConfirmNewPassword = null
);
