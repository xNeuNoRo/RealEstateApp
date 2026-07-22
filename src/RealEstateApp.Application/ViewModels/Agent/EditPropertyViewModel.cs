using RealEstateApp.Application.Dtos.Property.Responses;

namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class EditPropertyViewModel : CreatePropertyViewModel
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public IReadOnlyList<PropertyImageDto> ExistingImages { get; set; } = [];

    public List<int> ImageIdsToRemove { get; set; } = [];

}
