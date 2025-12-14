using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ToursAndTravelsManagement.Controllers
{
    [AllowAnonymous]
    public class CustomerInfoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
