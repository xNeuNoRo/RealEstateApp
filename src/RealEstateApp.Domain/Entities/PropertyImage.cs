using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa una imagen asociada a una propiedad.
/// </summary>
public class PropertyImage : BaseEntity
{
    public int PropertyId { get; private set; }
    public string Url { get; private set; } = null!;
    public bool IsMain { get; private set; }

    public Property Property { get; private set; } = null!;

    private PropertyImage() { }

    internal PropertyImage(int propertyId, string url, bool isMain)
    {
        PropertyId = propertyId;
        Url = url;
        IsMain = isMain;
    }

    internal void SetMain(bool value) => IsMain = value;

    internal void ChangeUrl(string url)
    {
        Url = url;
        Touch();
    }
}
