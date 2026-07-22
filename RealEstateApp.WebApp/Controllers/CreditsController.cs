using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.ViewModels.Credits;

namespace RealEstateApp.WebApp.Controllers;

[AllowAnonymous]
public class CreditsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new CreditsViewModel());
    }
}
