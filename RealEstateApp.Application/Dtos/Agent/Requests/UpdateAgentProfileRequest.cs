using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Dtos.Agent.Requests;

public sealed record UpdateAgentProfileRequest(
    string FirstName,
    string LastName,
    string Phone,
    IAppFile? PhotoFile
);
