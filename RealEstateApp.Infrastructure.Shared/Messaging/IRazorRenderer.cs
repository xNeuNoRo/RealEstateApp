namespace RealEstateApp.Infrastructure.Shared.Messaging;

/// <summary>
/// Renderizador de plantillas Razor para generar cuerpos HTML de correos.
/// </summary>
public interface IRazorRenderer
{
    Task<string> RenderTemplateAsync<T>(string templatePath, T model);
}
