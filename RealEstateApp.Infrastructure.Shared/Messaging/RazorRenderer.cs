using RazorLight;

namespace RealEstateApp.Infrastructure.Shared.Messaging;

/// <summary>
/// Renderizador de plantillas Razor usando RazorLight con caché en memoria.
/// </summary>
public class RazorRenderer : IRazorRenderer
{
    private readonly IRazorLightEngine _engine;

    public RazorRenderer()
    {
        _engine = new RazorLightEngineBuilder().UseMemoryCachingProvider().Build();
    }

    /// <summary>
    /// Renderiza una plantilla Razor desde archivo con el modelo proporcionado.
    /// </summary>
    public async Task<string> RenderTemplateAsync<T>(string templatePath, T model)
    {
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException(
                $"La plantilla de correo no se encontro en: {templatePath}"
            );
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        return await _engine.CompileRenderStringAsync(templatePath, templateContent, model);
    }
}
