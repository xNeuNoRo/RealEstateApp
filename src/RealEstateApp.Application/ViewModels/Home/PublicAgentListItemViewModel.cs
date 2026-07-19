namespace RealEstateApp.Application.ViewModels.Home;

public sealed class PublicAgentListItemViewModel
{
    public string Id { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string? ProfileImage { get; init; }
}
