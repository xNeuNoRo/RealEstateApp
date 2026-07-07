using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa un tipo de propiedad.
/// </summary>
public class PropertyType : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    private PropertyType() { }

    public static Result<PropertyType> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<PropertyType>(
                Error.Validation("PropertyType.Name", "El nombre es requerido.")
            );
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<PropertyType>(
                Error.Validation("PropertyType.Desc", "La descripción es requerida.")
            );
        var clean = name.Trim();
        if (clean.Length > 80)
            return Result.Failure<PropertyType>(
                Error.Validation(
                    "PropertyType.NameTooLong",
                    "El nombre no debe exceder 80 caracteres."
                )
            );

        return Result.Success(new PropertyType { Name = clean, Description = description.Trim() });
    }

    public Result Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("PropertyType.Name", "El nombre es requerido."));
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(
                Error.Validation("PropertyType.Desc", "La descripción es requerida.")
            );
        if (name.Trim().Length > 80)
            return Result.Failure(
                Error.Validation(
                    "PropertyType.NameTooLong",
                    "El nombre no debe exceder 80 caracteres."
                )
            );
        Name = name.Trim();
        Description = description.Trim();
        Touch();
        return Result.Success();
    }
}
