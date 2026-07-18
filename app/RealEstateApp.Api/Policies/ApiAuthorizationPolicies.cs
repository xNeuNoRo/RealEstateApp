using Microsoft.AspNetCore.Authorization;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Api.Policies;

public static class ApiAuthorizationPolicies
{
    public const string ApiAccess = "ApiAccess";
    public const string WebAppAccess = "WebAppAccess";

    public static void AddApiAuthorizationPolicies(this AuthorizationOptions options)
    {
        // Agregamos la politica de autorización para el acceso a la API,
        // que requiere que el usuario tenga el rol de Admin o Developer.
        options.AddPolicy(
            ApiAccess,
            policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.Developer))
        );

        // Agregamos la politica de autorización para el acceso a la WebApp,
        // que requiere que el usuario tenga el rol de Client, Agent o Admin.
        options.AddPolicy(
            WebAppAccess,
            policy =>
                policy.RequireRole(nameof(Roles.Client), nameof(Roles.Agent), nameof(Roles.Admin))
        );
    }
}
