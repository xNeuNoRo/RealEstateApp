using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Domain.Interfaces.Persistence.Repositories;

/// <summary>
/// Repositorio de Property. Extiende el genérico con consultas específicas de dominio.
/// </summary>
public interface IPropertyRepository : IGenericRepository<Property>
{
    Task<Property?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Property>> GetAvailableAsync(
        QueryOptions<Property>? options = null,
        CancellationToken ct = default
    );
    Task<IReadOnlyList<Property>> GetByAgentAsync(
        string agentId,
        PropertyStatus? status = null,
        QueryOptions<Property>? options = null,
        CancellationToken ct = default
    );
    Task<bool> IsPropertyOwnedByAgentAsync(
        int propertyId,
        string agentId,
        CancellationToken ct = default
    );
}
