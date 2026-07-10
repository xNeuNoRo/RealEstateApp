using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Interfaces.Events;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Domain.Interfaces.Services;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Persistence;
using RealEstateApp.Infrastructure.Persistence.Repositories;
using RealEstateApp.Infrastructure.Persistence.Services;
using RealEstateApp.Infrastructure.Persistence.Services.EventHandlers;

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
                sql =>
                {
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                    sql.CommandTimeout(30);
                }
            )
        );

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IFavoritePropertyRepository, FavoritePropertyRepository>();

        services.AddScoped<IPropertyCodeGenerator, PropertyCodeGenerator>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<
            IEventHandler<Domain.Events.OfferAcceptedEvent>,
            OfferAcceptedEventHandler
        >();

        services.AddScoped<IOfferPolicy, OfferPolicy>();

        return services;
    }
}
