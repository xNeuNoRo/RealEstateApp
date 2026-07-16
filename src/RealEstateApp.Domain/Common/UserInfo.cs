namespace RealEstateApp.Domain.Common;

public sealed record UserInfo
{
    public string Id { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? ProfileImage { get; init; }
    public string? UserName { get; init; }
}
