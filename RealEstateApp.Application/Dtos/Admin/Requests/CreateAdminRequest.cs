namespace RealEstateApp.Application.Dtos.Admin.Requests;

public sealed record CreateAdminRequest(
    string FirstName,
    string LastName,
    string IdentityDocument,
    string Email,
    string UserName,
    string Password,
    string ConfirmPassword
);
