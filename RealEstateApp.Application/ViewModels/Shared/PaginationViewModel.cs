namespace RealEstateApp.Application.ViewModels.Shared;

public sealed class PaginationViewModel
{
    public string Action { get; init; } = null!;
    public string? Controller { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyDictionary<string, object?> RouteValues { get; init; } =
        new Dictionary<string, object?>();
}
