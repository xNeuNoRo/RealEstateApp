using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Domain.Interfaces.Services;

/// <summary>
/// Servicio de dominio para generar códigos PropertyCode únicos de 6 dígitos.
/// La implementación en Infrastructure.Persistence consulta la BD para asegurar unicidad (MAX+1 con retry).
/// Llamado desde el Application Service antes de Property.Create.
/// </summary>
public interface IPropertyCodeGenerator
{
    Task<PropertyCode> GenerateNextAsync(CancellationToken ct = default);
}
