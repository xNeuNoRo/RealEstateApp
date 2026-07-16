using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Dtos.Client.Requests;

public sealed record UpdateClientProfileRequest(
    string FirstName,
    string LastName,
    string Phone,
    IAppFile? PhotoFile
);
