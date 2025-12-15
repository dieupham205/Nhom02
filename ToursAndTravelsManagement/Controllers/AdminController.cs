using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    public IActionResult Profile()
    {
        return View();
    }
    public IActionResult Dashboard()
{
    return View();
}
}

