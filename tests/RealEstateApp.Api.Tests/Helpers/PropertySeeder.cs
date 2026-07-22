using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Api.Tests.Helpers;

internal static class PropertySeeder
{
    internal static async Task SeedAsync(ApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();

        if (await db.Properties.AnyAsync())
            return;

        var userManager = sp.GetRequiredService<UserManager<AppUser>>();
        var developer =
            await userManager.FindByNameAsync("developer")
            ?? throw new InvalidOperationException("El usuario 'developer' no fue encontrado.");

        var props = new List<Property>
        {
            CreateProperty(
                PropertyCode.Unsafe("100001"),
                "Casa en Ensanche Piantini",
                "Hermosa casa con 3 habitaciones y piscina.",
                Price.Unsafe(4500000.50m, "DOP"),
                Size.Unsafe(150.5m, "m2"),
                3,
                2,
                1,
                1,
                developer.Id,
                [1, 4]
            ),
            CreateProperty(
                PropertyCode.Unsafe("100002"),
                "Apartamento en Naco",
                "Moderno apartamento cerca del centro.",
                Price.Unsafe(3200000.00m, "DOP"),
                Size.Unsafe(98.0m, "m2"),
                2,
                1,
                2,
                2,
                developer.Id,
                [2, 5]
            ),
            CreateProperty(
                PropertyCode.Unsafe("100003"),
                "Casa en Arroyo Hondo",
                "Casa familiar con amplio jardín.",
                Price.Unsafe(6200000.00m, "DOP"),
                Size.Unsafe(200.0m, "m2"),
                3,
                2,
                1,
                1,
                developer.Id,
                [1, 2]
            ),
            CreateProperty(
                PropertyCode.Unsafe("100004"),
                "Solar en Los Prados",
                "Amplio solar urbanizado.",
                Price.Unsafe(12000000.00m, "DOP"),
                Size.Unsafe(450.0m, "m2"),
                0,
                0,
                4,
                1,
                developer.Id,
                [3]
            ),
            CreateProperty(
                PropertyCode.Unsafe("100005"),
                "Local Comercial",
                "Local comercial en zona transitada.",
                Price.Unsafe(5500000.00m, "DOP"),
                Size.Unsafe(120.0m, "m2"),
                1,
                1,
                5,
                1,
                developer.Id,
                [4]
            ),
            CreateProperty(
                PropertyCode.Unsafe("100006"),
                "Habitación Zona Universitaria",
                "Habitación amueblada cerca de la U.",
                Price.Unsafe(1800000.00m, "DOP"),
                Size.Unsafe(25.0m, "m2"),
                1,
                1,
                3,
                1,
                developer.Id,
                [6]
            ),
        };

        db.Properties.AddRange(props);
        await db.SaveChangesAsync();
    }

    private static Property CreateProperty(
        PropertyCode code,
        string title,
        string description,
        Price price,
        Size size,
        int beds,
        int baths,
        int propertyTypeId,
        int saleTypeId,
        string agentId,
        int[] improvementIds
    )
    {
        return Property
            .Create(
                code,
                title,
                description,
                price,
                size,
                beds,
                baths,
                propertyTypeId,
                saleTypeId,
                agentId,
                ["https://picsum.photos/seed/test/800/600"],
                improvementIds
            )
            .Value!;
    }
}
