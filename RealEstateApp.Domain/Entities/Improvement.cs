using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa una mejora de una propiedad.
/// </summary>
public class Improvement : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    private Improvement() { }

    public static Result<Improvement> Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Improvement>(
                Error.Validation("Improvement.Name", "El nombre es requerido.")
            );
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Improvement>(
                Error.Validation("Improvement.Desc", "La descripción es requerida.")
            );
        var clean = name.Trim();
        if (clean.Length > 80)
            return Result.Failure<Improvement>(
                Error.Validation(
                    "Improvement.NameTooLong",
                    "El nombre no debe exceder 80 caracteres."
                )
            );

        return Result.Success(new Improvement { Name = clean, Description = description.Trim() });
    }

    public Result Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Improvement.Name", "El nombre es requerido."));
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(Error.Validation("Improvement.Desc", "La descripción es requerida."));
        if (name.Trim().Length > 80)
            return Result.Failure(Error.Validation("Improvement.NameTooLong", "El nombre no debe exceder 80 caracteres."));
        Name = name.Trim();
        Description = description.Trim();
        Touch();
        return Result.Success();
    }
}
