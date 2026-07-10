using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

namespace RealEstateApp.Infrastructure.Persistence.Contexts;

/// <summary>
/// Contexto principal de EF Core para la persistencia del dominio.
/// </summary>
public sealed class AppDbContext : DbContext
{
    private readonly IDateTimeProvider _timeProvider;
    private readonly ILogger<AppDbContext> _logger;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IDateTimeProvider timeProvider,
        ILogger<AppDbContext> logger
    )
        : base(options)
    {
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<PropertyImprovement> PropertyImprovements => Set<PropertyImprovement>();
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<SaleType> SaleTypes => Set<SaleType>();
    public DbSet<Improvement> Improvements => Set<Improvement>();
    public DbSet<FavoriteProperty> FavoriteProperties => Set<FavoriteProperty>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        CatalogSeeds.SeedPropertyTypes(modelBuilder);
        CatalogSeeds.SeedSaleTypes(modelBuilder);
        CatalogSeeds.SeedImprovements(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        AuditEntries();
        var result = base.SaveChangesAsync(cancellationToken);
        sw.Stop();
        if (sw.ElapsedMilliseconds > 100)
            _logger.LogWarning("SaveChangesAsync tardó {Duration}ms", sw.ElapsedMilliseconds);
        return result;
    }

    public override int SaveChanges()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        AuditEntries();
        var result = base.SaveChanges();
        sw.Stop();
        if (sw.ElapsedMilliseconds > 100)
            _logger.LogWarning("SaveChanges tardó {Duration}ms", sw.ElapsedMilliseconds);
        return result;
    }

    private void AuditEntries()
    {
        var now = _timeProvider.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = now;
            }
        }
    }
}
