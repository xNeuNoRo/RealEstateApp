using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class ImprovementListViewModel : BaseViewModel
{
    public PagedResult<ImprovementListItemViewModel> Items { get; init; } = new([], 0, 1, 20);
    public string? SearchTerm { get; init; }
}
