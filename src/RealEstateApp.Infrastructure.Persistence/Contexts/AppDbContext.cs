using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Contexts;

/// <summary>
/// Contexto principal de EF Core para la persistencia del dominio.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<PropertyImprovement> PropertyImprovements => Set<PropertyImprovement>();
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<SaleType> SaleTypes => Set<SaleType>();
    public DbSet<Improvement> Improvements => Set<Improvement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AuditEntries();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AuditEntries();
        return base.SaveChanges();
    }

    /// <summary>
    /// Actualiza <c>CreatedAt</c> en insert y <c>UpdatedAt</c> en update.
    /// </summary>
    private void AuditEntries()
    {
        const string createdAt = nameof(IAuditableEntity.CreatedAt);
        const string updatedAt = nameof(IAuditableEntity.UpdatedAt);

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(createdAt).CurrentValue = DateTimeOffset.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(updatedAt).CurrentValue = DateTimeOffset.UtcNow;
            }
        }
    }
}
