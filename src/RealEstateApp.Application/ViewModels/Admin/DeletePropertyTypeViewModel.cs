namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class DeletePropertyTypeViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public int PropertiesCount { get; init; }
}
