using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Interfaces.Services;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Services;

/// <summary>
/// Genera códigos PropertyCode únicos consultando el último código en BD.
/// </summary>
public sealed class PropertyCodeGenerator : IPropertyCodeGenerator
{
    private readonly AppDbContext _context;

    public PropertyCodeGenerator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyCode> GenerateNextAsync(CancellationToken ct = default)
    {
        var codes = await _context
            .Properties.AsNoTracking()
            .Select(x => x.Code.Value)
            .ToListAsync(ct);
        var lastCode = codes.OrderByDescending(c => c).FirstOrDefault();

        var next = lastCode switch
        {
            null => "100000",
            _ => (int.Parse(lastCode) + 1).ToString("D6"),
        };

        return PropertyCode.Unsafe(next);
    }
}
