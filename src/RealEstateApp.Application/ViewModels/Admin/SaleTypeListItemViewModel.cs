namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class SaleTypeListItemViewModel
{
    public int Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int PropertiesCount { get; init; }
}
