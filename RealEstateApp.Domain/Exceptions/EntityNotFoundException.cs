namespace RealEstateApp.Domain.Exceptions;

/// <summary>
/// Se lanza cuando una búsqueda de entidad por clave no devuelve resultados.
/// La capa de aplicación mapea esto a Result.NotFound.
/// </summary>
public class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    public object Key { get; }

    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} con clave '{key}' no fue encontrada.")
    {
        EntityName = entityName;
        Key = key;
    }
}
