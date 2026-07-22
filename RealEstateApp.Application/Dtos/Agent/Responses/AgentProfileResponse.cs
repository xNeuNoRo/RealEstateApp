namespace RealEstateApp.Application.Dtos.Agent.Responses;

public sealed class AgentProfileResponse
{
    public string Id { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? Phone { get; init; }
    public string? ProfileImage { get; init; }
}
