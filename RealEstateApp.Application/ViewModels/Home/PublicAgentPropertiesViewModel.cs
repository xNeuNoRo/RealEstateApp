using RealEstateApp.Application.ViewModels.Property;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.ViewModels.Home;

public sealed class PublicAgentPropertiesViewModel : BaseViewModel
{
    public PublicAgentListItemViewModel Agent { get; init; } = null!;
    public PagedResult<PropertyListItemViewModel> Properties { get; init; } =
        new(Array.Empty<PropertyListItemViewModel>(), 0, 1, 12);
}
