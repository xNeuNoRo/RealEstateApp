using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.WebApp.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
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

        var isActive = await currentUserService.IsActiveAsync(context.HttpContext.RequestAborted);
        if (!isActive)
        {
            SetErrorAndRedirect(
                context,
                "Su cuenta se encuentra inactiva. Verifique su correo electrónico para activarla.",
                "Account.Inactive"
            );
        }
    }

    private static void SetErrorAndRedirect(
        AuthorizationFilterContext context,
        string message,
        string code = "Session.Expired"
    )
    {
        var tempDataFactory =
            context.HttpContext.RequestServices.GetService<ITempDataDictionaryFactory>();
        if (tempDataFactory != null)
        {
            var tempData = tempDataFactory.GetTempData(context.HttpContext);
            tempData["ErrorMessage"] = message;
            tempData["ErrorCode"] = code;
            tempData.Save();
        }

        context.Result = new RedirectToRouteResult(
            new RouteValueDictionary(new { controller = "Auth", action = "Login" })
        );
    }
}
