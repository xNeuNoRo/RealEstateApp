using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.WebApp.Controllers;

public abstract class BaseController : Controller
{
    protected BaseController(ICurrentUserService currentUser)
    {
        CurrentUser = currentUser;
    }

    protected ICurrentUserService CurrentUser { get; }
}
