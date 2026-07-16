namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class ImprovementListItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int PropertiesCount { get; init; }
}
