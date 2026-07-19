using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class DeveloperListViewModel : BaseViewModel
{
    public PagedResult<DeveloperListItemViewModel> Items { get; init; } = new([], 0, 0, 0);
    public string? SearchTerm { get; init; }
}
