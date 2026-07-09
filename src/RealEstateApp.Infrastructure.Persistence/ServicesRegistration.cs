using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence;

public static class ServicesRegistration
{
    /// <summary>
    /// Registra <see cref="AppDbContext"/> con SQL Server y el ensamblado de migraciones.
    /// Repositorios y UnitOfWork se registran cuando existan sus implementaciones.
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("RealEstateDb"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            )
        );

        return services;
    }
}
