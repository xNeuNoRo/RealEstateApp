using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Application.Interfaces.UseCases.Chat;
using RealEstateApp.Application.Interfaces.UseCases.Client;
using RealEstateApp.Application.Interfaces.UseCases.Favorites;
using RealEstateApp.Application.Interfaces.UseCases.Offers;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Application.UseCases.Catalog;
using RealEstateApp.Application.UseCases.Chat;
using RealEstateApp.Application.UseCases.Client;
using RealEstateApp.Application.UseCases.Favorites;
using RealEstateApp.Application.UseCases.Improvement;
using RealEstateApp.Application.UseCases.Offers;
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

    public static IServiceCollection AddFavoriteUseCases(this IServiceCollection services)
    {
        services.AddScoped<IAddFavoriteUseCase, AddFavoriteUseCase>();
        services.AddScoped<IGetMyFavoritesUseCase, GetMyFavoritesUseCase>();
        services.AddScoped<IRemoveFavoriteUseCase, RemoveFavoriteUseCase>();

        return services;
    }

    public static IServiceCollection AddChatUseCases(this IServiceCollection services)
    {
        services.AddScoped<ISendMessageUseCase, SendMessageUseCase>();
        services.AddScoped<IReplyMessageUseCase, ReplyMessageUseCase>();
        services.AddScoped<IGetConversationUseCase, GetConversationUseCase>();
        services.AddScoped<IGetMyConversationsUseCase, GetMyConversationsUseCase>();

        return services;
    }

    public static IServiceCollection AddOfferUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICreateOfferUseCase, CreateOfferUseCase>();
        services.AddScoped<IAcceptOfferUseCase, AcceptOfferUseCase>();
        services.AddScoped<IRejectOfferUseCase, RejectOfferUseCase>();
        services.AddScoped<IGetPropertyOffersUseCase, GetPropertyOffersUseCase>();
        services.AddScoped<IGetMyOffersUseCase, GetMyOffersUseCase>();

        return services;
    }

    public static IServiceCollection AddClientUseCases(this IServiceCollection services)
    {
        services.AddScoped<IGetClientDashboardUseCase, GetClientDashboardUseCase>();
        services.AddScoped<IGetClientProfileUseCase, GetClientProfileUseCase>();

        return services;
    }

    public static IServiceCollection AddCatalogUseCases(this IServiceCollection services)
    {
        services.AddScoped<ICreateImprovementUseCase, CreateImprovementUseCase>();
        services.AddScoped<IUpdateImprovementUseCase, UpdateImprovementUseCase>();
        services.AddScoped<IGetAllImprovementsUseCase, GetAllImprovementsUseCase>();
        services.AddScoped<IDeleteImprovementUseCase, DeleteImprovementUseCase>();

        services.AddScoped<ICreatePropertyTypeUseCase, CreatePropertyTypeUseCase>();
        services.AddScoped<IUpdatePropertyTypeUseCase, UpdatePropertyTypeUseCase>();
        services.AddScoped<IGetAllPropertyTypesUseCase, GetAllPropertyTypesUseCase>();
        services.AddScoped<IDeletePropertyTypeUseCase, DeletePropertyTypeUseCase>();

        // SaleType
        services.AddScoped<ICreateSaleTypeUseCase, CreateSaleTypeUseCase>();
        services.AddScoped<IUpdateSaleTypeUseCase, UpdateSaleTypeUseCase>();
        services.AddScoped<IGetAllSaleTypesUseCase, GetAllSaleTypesUseCase>();
        services.AddScoped<IDeleteSaleTypeUseCase, DeleteSaleTypeUseCase>();

        return services;
    }
}
