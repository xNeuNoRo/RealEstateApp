namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class DeletePropertyViewModel : BaseViewModel
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string? MainImageUrl { get; init; }
}
