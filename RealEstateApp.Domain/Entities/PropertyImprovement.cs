using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa la relación entre una propiedad y una mejora asociada a ella.
/// </summary>
public class PropertyImprovement
{
    public int PropertyId { get; private set; }
    public int ImprovementId { get; private set; }

    public Property Property { get; private set; } = null!;
    public Improvement Improvement { get; private set; } = null!;

    private PropertyImprovement() { }

    internal PropertyImprovement(int propertyId, int improvementId)
    {
        PropertyId = propertyId;
        ImprovementId = improvementId;
    }
}
