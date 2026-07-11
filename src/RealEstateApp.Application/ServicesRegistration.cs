using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Application.UseCases.Property;

namespace RealEstateApp.Application;

/// <summary>
/// Punto de entrada para el registro de la capa de Application en el contenedor de DI.
/// Registra AutoMapper (profiles), FluentValidation (validadores) y expone el método
/// para registrar los Use Cases específicos desde cada módulo de feature.
/// </summary>
public static class ServicesRegistration
{
    /// <summary>
    /// Registra AutoMapper y FluentValidation del ensamblado de Application.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(_ => { }, Assembly.GetExecutingAssembly());
        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IServiceCollection AddPropertyUseCases(this IServiceCollection services)
    {
        services.AddScoped<IGetPropertyListUseCase, GetPropertyListUseCase>();
        services.AddScoped<ISearchPropertyByCodeUseCase, SearchPropertyByCodeUseCase>();
        services.AddScoped<IGetPropertyDetailUseCase, GetPropertyDetailUseCase>();
        services.AddScoped<IGetAgentPropertiesUseCase, GetAgentPropertiesUseCase>();
        services.AddScoped<ICreatePropertyUseCase, CreatePropertyUseCase>();
        services.AddScoped<IUpdatePropertyUseCase, UpdatePropertyUseCase>();
        services.AddScoped<IDeletePropertyUseCase, DeletePropertyUseCase>();

        return services;
    }
}
