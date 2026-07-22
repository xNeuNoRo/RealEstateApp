using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Domain.Entities;

/// <summary>
/// Representa un tipo de venta de propiedad.
/// </summary>
public class SaleType : AggregateRoot
{
    public SaleTypeCode Code { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    private SaleType() { }

    public static Result<SaleType> Create(SaleTypeCode code, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<SaleType>(
                Error.Validation("SaleType.Name", "El nombre es requerido.")
            );
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<SaleType>(
                Error.Validation("SaleType.Desc", "La descripción es requerida.")
            );
        if (name.Trim().Length > 80)
            return Result.Failure<SaleType>(
                Error.Validation("SaleType.NameTooLong", "El nombre no debe exceder 80 caracteres.")
            );
        if (!Enum.IsDefined(code))
            return Result.Failure<SaleType>(
                Error.Validation("SaleType.Code", "Código de tipo de venta fuera de rango.")
            );

        return Result.Success(
            new SaleType
            {
                Code = code,
                Name = name.Trim(),
                Description = description.Trim(),
            }
        );
    }

    public Result Update(SaleTypeCode code, string name, string description)
    {
        if (!Enum.IsDefined(code))
            return Result.Failure(
                Error.Validation("SaleType.Code", "Código de tipo de venta fuera de rango.")
            );
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("SaleType.Name", "El nombre es requerido."));
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(
                Error.Validation("SaleType.Desc", "La descripción es requerida.")
            );
        if (name.Trim().Length > 80)
            return Result.Failure(
                Error.Validation("SaleType.NameTooLong", "El nombre no debe exceder 80 caracteres.")
            );
        Code = code;
        Name = name.Trim();
        Description = description.Trim();
        Touch();
        return Result.Success();
    }
}
