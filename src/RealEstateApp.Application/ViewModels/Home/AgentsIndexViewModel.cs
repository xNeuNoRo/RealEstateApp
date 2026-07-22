using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.ViewModels.Home;

public sealed class AgentsIndexViewModel : BaseViewModel
{
    public string? SearchTerm { get; init; }
    public PagedResult<PublicAgentListItemViewModel> Agents { get; init; } =
        new(Array.Empty<PublicAgentListItemViewModel>(), 0, 1, 12);
}
