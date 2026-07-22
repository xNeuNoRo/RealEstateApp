namespace RealEstateApp.Api.Dtos.Api.Responses;

public sealed class AgentApiResponse
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public int PropertiesCount { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public bool Status { get; init; }
}
