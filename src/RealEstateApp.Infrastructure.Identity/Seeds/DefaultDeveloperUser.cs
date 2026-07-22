using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Seeds;

/// <summary>
/// Crea el usuario Desarrollador por defecto para la API.
/// Lee credenciales desde IConfiguration (sección SeedData).
/// </summary>
public static class DefaultDeveloperUser
{
    public static async Task SeedAsync(
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        ILogger? logger = null
    )
    {
        var userName = configuration["SeedData:DeveloperUserName"] ?? "developer";
        if (await userManager.FindByNameAsync(userName) is not null)
            return;

        var user = new AppUser
        {
            UserName = userName,
            Email = configuration["SeedData:DeveloperEmail"] ?? "dev@realestate.local",
            EmailConfirmed = true,
            Active = true,
            FirstName = configuration["SeedData:DeveloperFirstName"] ?? "System",
            LastName = configuration["SeedData:DeveloperLastName"] ?? "Developer",
            IdentityDocument = configuration["SeedData:DeveloperIdentityDocument"] ?? "00212345672",
        };

        var password = configuration["SeedData:DeveloperPassword"] ?? "Developer123!";
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, DefaultRoles.Developer);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger?.LogError("Fallo al crear usuario developer '{User}': {Errors}", userName, errors);
        }
    }
}
