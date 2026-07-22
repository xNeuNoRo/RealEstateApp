using Microsoft.AspNetCore.Authorization;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Api.Policies;

public static class ApiAuthorizationPolicies
{
    public const string ApiAccess = "ApiAccess";

    public static void AddApiAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(
            ApiAccess,
            policy => policy.RequireRole(nameof(Roles.Admin), nameof(Roles.Developer))
        );
    }
}
