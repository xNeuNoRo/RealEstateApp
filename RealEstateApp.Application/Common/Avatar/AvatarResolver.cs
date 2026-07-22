namespace RealEstateApp.Application.Common.Avatar;

public static class AvatarResolver
{
    public const string DefaultAvatarPath = "/images/default-avatar.png";

    private static readonly string[] Icons =
    [
        "user",
        "user-circle",
        "user-round",
        "user-check",
        "building-2",
        "home",
        "store",
        "star",
        "shield",
        "key",
        "landmark",
        "compass",
        "crown",
        "gem",
    ];

    private static readonly (string from, string to)[] Palettes =
    [
        ("blue-600", "blue-700"),
        ("blue-500", "cyan-600"),
        ("teal-500", "emerald-600"),
        ("emerald-500", "green-600"),
        ("amber-500", "orange-600"),
        ("rose-500", "pink-600"),
        ("violet-500", "purple-700"),
        ("cyan-500", "blue-600"),
        ("slate-600", "gray-700"),
        ("indigo-500", "blue-600"),
    ];

    public static bool IsDefaultAvatar(string? path) =>
        string.IsNullOrWhiteSpace(path) || path == DefaultAvatarPath;

    public static string GetIcon(string name) => Icons[Math.Abs(name.GetHashCode()) % Icons.Length];

    public static (string from, string to) GetPalette(string name) =>
        Palettes[Math.Abs(name.GetHashCode()) % Palettes.Length];

    public static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "U";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = string.Concat(parts.Select(w => w[0])).ToUpperInvariant();
        return initials.Length > 2 ? initials[..2] : initials;
    }
}
