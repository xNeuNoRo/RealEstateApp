using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Infrastructure.Shared.Messaging;
using RealEstateApp.Infrastructure.Shared.Storage;

namespace RealEstateApp.Infrastructure.Shared;

/// <summary>
/// Registro de servicios de infraestructura compartida (correo y archivos).
/// </summary>
public static class ServicesRegistration
{
    /// <summary>
    /// Registra MailSettings, FileSettings, RazorRenderer, MailKitEmailService y FileService.
    /// </summary>
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<MailSettings>(configuration.GetSection(MailSettings.SectionName));
        services.Configure<FileSettings>(configuration.GetSection(FileSettings.SectionName));

        services.AddSingleton<IRazorRenderer, RazorRenderer>();
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}
