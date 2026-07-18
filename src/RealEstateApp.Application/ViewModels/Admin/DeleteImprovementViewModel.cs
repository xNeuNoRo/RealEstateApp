namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class DeleteImprovementViewModel : BaseViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int PropertiesCount { get; init; }
}
