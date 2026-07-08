using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa una propiedad inmobiliaria en el sistema.
/// </summary>
public class Property : AggregateRoot
{
    private readonly List<PropertyImage> _images = new();
    private readonly List<PropertyImprovement> _improvements = new();

    public PropertyCode Code { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Price Price { get; private set; } = null!;
    public Size Size { get; private set; } = null!;
    public int Bedrooms { get; private set; }
    public int Bathrooms { get; private set; }
    public PropertyStatus Status { get; private set; }

    public int PropertyTypeId { get; private set; }
    public PropertyType? PropertyType { get; private set; }

    public int SaleTypeId { get; private set; }
    public SaleType? SaleType { get; private set; }

    public string AgentId { get; private set; } = null!;

    public IReadOnlyCollection<PropertyImage> Images => _images.AsReadOnly();
    public IReadOnlyCollection<PropertyImprovement> Improvements => _improvements.AsReadOnly();

    private Property() { }

    public static Result<Property> Create(
        PropertyCode code,
        string description,
        Price price,
        Size size,
        int bedrooms,
        int bathrooms,
        int propertyTypeId,
        int saleTypeId,
        string agentId,
        IReadOnlyCollection<string> initialImageUrls,
        IReadOnlyCollection<int> initialImprovementIds
    )
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Property>(
                Error.Validation("Property.Desc", "La descripción es requerida.")
            );
        if (bedrooms < 0)
            return Result.Failure<Property>(
                Error.Validation("Property.Bedrooms", "Las habitaciones no pueden ser negativas.")
            );
        if (bathrooms < 0)
            return Result.Failure<Property>(
                Error.Validation("Property.Bathrooms", "Los baños no pueden ser negativos.")
            );
        if (propertyTypeId <= 0)
            return Result.Failure<Property>(
                Error.Validation("Property.PropertyType", "El tipo de propiedad es requerido.")
            );
        if (saleTypeId <= 0)
            return Result.Failure<Property>(
                Error.Validation("Property.SaleType", "El tipo de venta es requerido.")
            );
        if (string.IsNullOrWhiteSpace(agentId))
            return Result.Failure<Property>(
                Error.Validation("Property.Agent", "El agente es requerido.")
            );

        if (initialImageUrls is null || initialImageUrls.Count == 0)
            return Result.Failure<Property>(
                Error.Validation("Property.Images", "Al menos una imagen es requerida.")
            );
        if (initialImageUrls.Count > 4)
            return Result.Failure<Property>(
                Error.Validation("Property.Images", "No puede exceder 4 imágenes.")
            );
        if (initialImprovementIds is null || initialImprovementIds.Count == 0)
            return Result.Failure<Property>(
                Error.Validation("Property.Improvements", "Al menos una mejora es requerida.")
            );

        var property = new Property
        {
            Code = code,
            Description = description.Trim(),
            Price = price,
            Size = size,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            Status = PropertyStatus.Available,
            PropertyTypeId = propertyTypeId,
            SaleTypeId = saleTypeId,
            AgentId = agentId,
        };

        var urlsList = initialImageUrls.ToList();
        for (int i = 0; i < urlsList.Count; i++)
            property._images.Add(new PropertyImage(property.Id, urlsList[i], isMain: i == 0));

        foreach (var impId in initialImprovementIds.Distinct())
            property._improvements.Add(new PropertyImprovement(property.Id, impId));

        return Result.Success(property);
    }

    public Result UpdatePrice(Price newPrice)
    {
        if (newPrice is null)
            return Result.Failure(Error.Validation("Property.Price", "El precio es requerido."));
        Price = newPrice;
        Touch();
        return Result.Success();
    }

    public Result AddImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Result.Failure(
                Error.Validation("Property.Image", "La URL de la imagen es requerida.")
            );
        if (_images.Count >= 4)
            return Result.Failure(
                Error.Conflict("Property.ImagesFull", "La propiedad ya tiene 4 imágenes.")
            );
        _images.Add(new PropertyImage(Id, url, isMain: false));
        Touch();
        return Result.Success();
    }

    public Result RemoveImage(int imageId)
    {
        var img = _images.FirstOrDefault(i => i.Id == imageId);
        if (img is null)
            return Result.Failure(Error.NotFound("Property.Image", "Imagen no encontrada."));
        var wasMain = img.IsMain;
        _images.Remove(img);
        if (wasMain && _images.Count > 0)
            _images[0].SetMain(true);
        if (_images.Count == 0)
            return Result.Failure(
                Error.Validation(
                    "Property.NoImages",
                    "La propiedad debe mantener al menos una imagen."
                )
            );
        Touch();
        return Result.Success();
    }

    public Result SetMainImage(int imageId)
    {
        var target = _images.FirstOrDefault(i => i.Id == imageId);
        if (target is null)
            return Result.Failure(Error.NotFound("Property.Image", "Imagen no encontrada."));
        foreach (var i in _images)
            i.SetMain(false);
        target.SetMain(true);
        Touch();
        return Result.Success();
    }

    public Result AddImprovement(int improvementId)
    {
        if (improvementId <= 0)
            return Result.Failure(
                Error.Validation("Property.ImprovementId", "La mejora es requerida.")
            );
        if (_improvements.Any(pi => pi.ImprovementId == improvementId))
            return Result.Failure(
                Error.Conflict("Property.DuplicateImprovement", "La mejora ya está vinculada.")
            );
        _improvements.Add(new PropertyImprovement(Id, improvementId));
        Touch();
        return Result.Success();
    }

    public Result RemoveImprovement(int improvementId)
    {
        var link = _improvements.FirstOrDefault(pi => pi.ImprovementId == improvementId);
        if (link is null)
            return Result.Failure(Error.NotFound("Property.Improvement", "Mejora no vinculada."));
        if (_improvements.Count == 1)
            return Result.Failure(
                Error.Validation("Property.LastImprovement", "Debe mantener al menos una mejora.")
            );
        _improvements.Remove(link);
        Touch();
        return Result.Success();
    }

    public void MarkAsSold()
    {
        Status = PropertyStatus.Sold;
        Touch();
    }
}
