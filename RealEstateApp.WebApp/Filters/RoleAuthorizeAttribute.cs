using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.WebApp.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RoleAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var currentUserService =
            context.HttpContext.RequestServices.GetService(typeof(ICurrentUserService))
            as ICurrentUserService;

        if (currentUserService == null || !currentUserService.IsAuthenticated)
        {
            context.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Auth", action = "Login" })
            );
            return;
        }

        if (!_roles.Any(r => currentUserService.IsInRole(r)))
        {
            context.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Auth", action = "AccessDenied" })
            );
        }
    }
}
