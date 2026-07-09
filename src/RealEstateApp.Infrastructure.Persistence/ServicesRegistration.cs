using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Domain.Interfaces.Services;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Persistence;
using RealEstateApp.Infrastructure.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Services;

namespace RealEstateApp.Infrastructure.Persistence;

public static class ServicesRegistration
{
    /// <summary>
    /// Registra <see cref="AppDbContext"/> con SQL Server, UnitOfWork y repositorios.
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

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IFavoritePropertyRepository, FavoritePropertyRepository>();
        services.AddScoped<IGenericRepository<PropertyType>, PropertyTypeRepository>();
        services.AddScoped<IGenericRepository<SaleType>, SaleTypeRepository>();
        services.AddScoped<IGenericRepository<Improvement>, ImprovementRepository>();

        services.AddScoped<IPropertyCodeGenerator, PropertyCodeGenerator>();

        return services;
    }
}
