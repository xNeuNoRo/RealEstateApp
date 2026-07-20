using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Interfaces.Events;
using RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

namespace RealEstateApp.Infrastructure.Persistence.Contexts;

/// <summary>
/// Contexto principal de EF Core para la persistencia del dominio.
/// </summary>
public sealed class AppDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AppDbContext> _logger;
    private readonly IDomainEventDispatcher _dispatcher;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        TimeProvider timeProvider,
        ILogger<AppDbContext> logger,
        IDomainEventDispatcher dispatcher
    )
        : base(options)
    {
        _timeProvider = timeProvider;
        _logger = logger;
        _dispatcher = dispatcher;
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        AuditEntries();
        // Primeramente, persistimos los cambios
        var result = await base.SaveChangesAsync(cancellationToken);

        // Luego, despachamos los eventos de dominio
        try
        {
            await DispatchDomainEventsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al despachar eventos de dominio tras SaveChanges.");
            throw;
        }
        // Persistimos nuevamente para guardar cualquier cambio realizado por los handlers de eventos
        await base.SaveChangesAsync(cancellationToken);
        sw.Stop();
        if (sw.ElapsedMilliseconds > 100)
            _logger.LogWarning("SaveChangesAsync tardó {Duration}ms", sw.ElapsedMilliseconds);
        return result;
    }

    public override int SaveChanges()
    {
        // Le arrojamos un mensaje q indique que no se soporta el SaveChanges síncrono,
        // ya que el dispatch de eventos de dominio requiere async.
        throw new NotSupportedException(
            "SaveChanges() síncrono no está soportado. Usa SaveChangesAsync(). "
                + "El dispatch de eventos de dominio requiere async."
        );
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                var method = typeof(IDomainEventDispatcher)
                    .GetMethod(nameof(IDomainEventDispatcher.DispatchAsync))!
                    .MakeGenericMethod(eventType);

                await (Task)method.Invoke(_dispatcher, [@event, ct])!;
            }
        }
    }

    private void AuditEntries()
    {
        var now = _timeProvider.GetUtcNow();

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
