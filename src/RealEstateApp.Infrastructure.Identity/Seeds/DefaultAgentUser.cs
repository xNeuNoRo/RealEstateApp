using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Seeds;

/// <summary>
/// Crea el usuario Agente por defecto para la WebApp.
/// Lee credenciales desde IConfiguration (sección SeedData).
/// </summary>
public static class DefaultAgentUser
{
    public static async Task SeedAsync(
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        ILogger? logger = null
    )
    {
        var userName = configuration["SeedData:AgentUserName"] ?? "agente";
        if (await userManager.FindByNameAsync(userName) is not null)
            return;

        var user = new AppUser
        {
            UserName = userName,
            Email = configuration["SeedData:AgentEmail"] ?? "agente@realestate.local",
            EmailConfirmed = true,
            Active = true,
            FirstName = configuration["SeedData:AgentFirstName"] ?? "María",
            LastName = configuration["SeedData:AgentLastName"] ?? "García",
            IdentityDocument = configuration["SeedData:AgentIdentityDocument"] ?? "00412345670",
        };

        var password = configuration["SeedData:AgentPassword"] ?? "Agente123!";
        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, DefaultRoles.Agent);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger?.LogError("Fallo al crear usuario agente '{User}': {Errors}", userName, errors);
        }
    }
}
