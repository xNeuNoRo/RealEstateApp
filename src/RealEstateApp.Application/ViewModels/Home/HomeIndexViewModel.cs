using RealEstateApp.Application.ViewModels.Property;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.ViewModels.Home;

public sealed class HomeIndexViewModel : BaseViewModel
{
    public PropertyFilterViewModel Filters { get; init; } = new();
    public PagedResult<PropertyListItemViewModel> Properties { get; init; } =
        new(Array.Empty<PropertyListItemViewModel>(), 0, 1, 12);

    public bool HasActiveFilters =>
        Filters.PropertyTypeId.HasValue
        || Filters.PriceMin.HasValue
        || Filters.PriceMax.HasValue
        || Filters.Bedrooms.HasValue
        || Filters.Bathrooms.HasValue
        || !string.IsNullOrWhiteSpace(Filters.Code);
}
