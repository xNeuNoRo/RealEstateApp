namespace RealEstateApp.Application.Dtos.Admin.Requests;

public sealed record UpdateAdminRequest(
    string AdminId,
    string FirstName,
    string LastName,
    string IdentityDocument,
    string Email,
    string UserName,
    string? NewPassword = null,
    string? ConfirmNewPassword = null
);
