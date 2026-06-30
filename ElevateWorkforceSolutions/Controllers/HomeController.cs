using Microsoft.AspNetCore.Mvc;

namespace ElevateWorkforceSolutions.Controllers;

/// <summary>Home page controller.</summary>
public class HomeController : Controller
{
    /// <summary>Public landing page with call-to-action links.</summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>Generic error page.</summary>
    public IActionResult Error()
    {
        return View();
    }
}
