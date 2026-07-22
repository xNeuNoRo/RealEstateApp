using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Seeds;

/// <summary>
/// Crea el usuario Administrador por defecto para la API.
/// Lee credenciales desde IConfiguration (sección SeedData).
/// </summary>
public static class DefaultAdminUser
{
    public static async Task SeedAsync(
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        ILogger? logger = null
    )
    {
        var userName = configuration["SeedData:AdminUserName"] ?? "admin";
        if (await userManager.FindByNameAsync(userName) is not null)
            return;

        var user = new AppUser
        {
            UserName = userName,
            Email = configuration["SeedData:AdminEmail"] ?? "admin@realestate.local",
            EmailConfirmed = true,
            Active = true,
            FirstName = configuration["SeedData:AdminFirstName"] ?? "System",
            LastName = configuration["SeedData:AdminLastName"] ?? "Admin",
            IdentityDocument = configuration["SeedData:AdminIdentityDocument"] ?? "00112345673",
        };

        var password = configuration["SeedData:AdminPassword"] ?? "Admin123!";
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, DefaultRoles.Admin);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger?.LogError("Fallo al crear usuario admin '{User}': {Errors}", userName, errors);
        }
    }
}
