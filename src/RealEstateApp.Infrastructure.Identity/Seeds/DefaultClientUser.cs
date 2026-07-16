using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Seeds;

/// <summary>
/// Crea el usuario Cliente por defecto para la WebApp.
/// Lee credenciales desde IConfiguration (sección SeedData).
/// </summary>
public static class DefaultClientUser
{
    public static async Task SeedAsync(
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        ILogger? logger = null
    )
    {
        var userName = configuration["SeedData:ClientUserName"] ?? "cliente";
        if (await userManager.FindByNameAsync(userName) is not null)
            return;

        var user = new AppUser
        {
            UserName = userName,
            Email = configuration["SeedData:ClientEmail"] ?? "cliente@realestate.local",
            EmailConfirmed = true,
            Active = true,
            FirstName = configuration["SeedData:ClientFirstName"] ?? "Juan",
            LastName = configuration["SeedData:ClientLastName"] ?? "Pérez",
            IdentityDocument = configuration["SeedData:ClientIdentityDocument"] ?? "40212345673",
        };

        var password = configuration["SeedData:ClientPassword"] ?? "Cliente123!";
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, DefaultRoles.Client);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger?.LogError("Fallo al crear usuario cliente '{User}': {Errors}", userName, errors);
        }
    }
}
