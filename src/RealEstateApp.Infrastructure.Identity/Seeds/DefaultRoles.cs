using Microsoft.AspNetCore.Identity;

namespace RealEstateApp.Infrastructure.Identity.Seeds;

/// <summary>
/// Crea los 4 roles del sistema si no existen.
/// </summary>
public static class DefaultRoles
{
    public const string Client = "Client";
    public const string Agent = "Agent";
    public const string Admin = "Admin";
    public const string Developer = "Developer";

    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { Client, Agent, Admin, Developer })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
