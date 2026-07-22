namespace RealEstateApp.WebApp.Helpers;

public static class PropertyHelper
{
    public static string GetStatusIcon(string? status) => status?.ToLowerInvariant() switch
    {
        "available" => "circle",
        "pending" => "clock",
        "sold" => "check-circle",
        "rented" => "file-text",
        _ => "help-circle"
    };

    public static string GetStatusColor(string? status) => status?.ToLowerInvariant() switch
    {
        "available" => "badge-success",
        "pending" => "badge-warning",
        "sold" => "badge-danger",
        "rented" => "badge-info",
        _ => "badge-default"
    };

    public static string GetPropertyTypeIcon(string? type) => type?.ToLowerInvariant() switch
    {
        "casa" or "house" => "home",
        "apartamento" or "apartment" => "building",
        "local" or "commercial" => "store",
        "terreno" or "land" => "map-pin",
        "oficina" or "office" => "briefcase",
        _ => "building-2"
    };

    public static string FormatBedrooms(int count) => count == 0 ? "Estudio" : $"{count} hab.";
}
