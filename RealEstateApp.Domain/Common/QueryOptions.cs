using System.Linq.Expressions;

namespace RealEstateApp.Domain.Common;

/// <summary>
/// Opciones de consulta para repositorios: filtro, includes, ordenamiento, paginación y tracking.
/// </summary>
public sealed class QueryOptions<T>
{
    public Expression<Func<T, bool>>? Filter { get; set; }
    public List<Expression<Func<T, object>>> Includes { get; set; } = new();
    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public bool IsTracking { get; set; }
    public bool UseSplitQuery { get; set; }
}
