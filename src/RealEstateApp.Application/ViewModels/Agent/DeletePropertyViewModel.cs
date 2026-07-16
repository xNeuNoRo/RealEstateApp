namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class DeletePropertyViewModel
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string? MainImageUrl { get; init; }
}
