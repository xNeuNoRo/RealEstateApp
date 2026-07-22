using Microsoft.EntityFrameworkCore;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public static class CatalogSeeds
{
    public static void SeedPropertyTypes(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<PropertyType>()
            .HasData(
                new
                {
                    Id = 1,
                    Name = "Casa",
                    Description = "Propiedad residencial unifamiliar.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 2,
                    Name = "Apartamento",
                    Description = "Unidad residencial en edificio multifamiliar.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 3,
                    Name = "Villa",
                    Description = "Propiedad residencial de lujo con amplios espacios.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 4,
                    Name = "Solar",
                    Description = "Terreno sin construcción.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 5,
                    Name = "Local comercial",
                    Description = "Espacio destinado a actividades comerciales.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                }
            );
    }

    public static void SeedSaleTypes(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<SaleType>()
            .HasData(
                new
                {
                    Id = 1,
                    Code = Domain.Enums.SaleTypeCode.Sale,
                    Name = "Venta",
                    Description = "Transacción de transferencia de propiedad.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 2,
                    Code = Domain.Enums.SaleTypeCode.Rent,
                    Name = "Alquiler",
                    Description = "Contrato de uso temporal de la propiedad.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 3,
                    Code = Domain.Enums.SaleTypeCode.RentToOwn,
                    Name = "Alquiler con opción a compra",
                    Description = "Alquiler con derecho a compra futura.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                }
            );
    }

    public static void SeedImprovements(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Improvement>()
            .HasData(
                new
                {
                    Id = 1,
                    Name = "Piscina",
                    Description = "Piscina privada o comunitaria.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 2,
                    Name = "Terraza",
                    Description = "Espacio al aire libre adicional.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 3,
                    Name = "Marquesina",
                    Description = "Estacionamiento techado para vehiculos.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 4,
                    Name = "Seguridad 24 horas",
                    Description = "Vigilancia y control de acceso permanente.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 5,
                    Name = "Ascensor",
                    Description = "Elevador en edificio o residencia.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                },
                new
                {
                    Id = 6,
                    Name = "Jardin",
                    Description = "Area verde paisajistica.",
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    UpdatedAt = (DateTimeOffset?)null,
                }
            );
    }
}
