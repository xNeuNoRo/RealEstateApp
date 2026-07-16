namespace RealEstateApp.Application.ViewModels.Shared;

public sealed class SelectListItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public bool Selected { get; init; }
}
