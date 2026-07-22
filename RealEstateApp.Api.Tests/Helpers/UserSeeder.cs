using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Api.Tests.Helpers;

internal static class UserSeeder
{
    private static bool _seeded;

    internal static async Task SeedAuthTestUsersAsync(ApiFactory factory)
    {
        if (_seeded)
            return;
        _seeded = true;

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var users = new (
            string userName,
            string password,
            string role,
            bool active,
            string cedula
        )[]
        {
            ("inactive_dev", "InactiveDev1!", "Developer", false, "00100000001"),
            ("test_client", "TestClient1!", "Client", true, "00200000002"),
            ("inactive_client", "InactiveClient1!", "Client", false, "00300000003"),
            ("inactive_agent", "InactiveAgent1!", "Agent", false, "00400000004"),
        };

        foreach (var (userName, password, role, active, cedula) in users)
        {
            if (await userManager.FindByNameAsync(userName) is not null)
                continue;

            var user = new AppUser
            {
                UserName = userName,
                Email = $"{userName}@test.com",
                FirstName = $"Test{role}",
                LastName = "User",
                IdentityDocument = cedula,
                Active = active,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Fallo al seedear el usuario {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Fallo al asignar el rol {role} a {userName}");
        }
    }
}
