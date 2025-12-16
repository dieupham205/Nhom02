using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Services.EmailService;

namespace ToursAndTravelsManagement.Controllers;

public class HomeController : Controller
{
    private readonly IEmailService _emailService;

    public HomeController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    // GET: Home/Index
    public IActionResult Index()
    {
        Log.Information("Home page (Index) accessed by user {UserId} at {Timestamp}", User.Identity?.Name, DateTime.Now);
        return View();
    }

    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        Log.Information("Privacy page accessed by user {UserId} at {Timestamp}", User.Identity?.Name, DateTime.Now);
        return View();
    }

    // GET: Home/Error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        Log.Error("Error page accessed. RequestId: {RequestId}, User: {UserId}, Timestamp: {Timestamp}", requestId, User.Identity?.Name, DateTime.Now);

        return View(new ErrorViewModel { RequestId = requestId });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitForm(ContactFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model); // Return to form view with model state
        }

        // Load the email template
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emailTemplates", "emailTemplate.html");
        var emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

        // Replace placeholders with actual values
        emailTemplate = emailTemplate.Replace("{{FirstName}}", model.FirstName)
                                     .Replace("{{LastName}}", model.LastName)
                                     .Replace("{{Email}}", model.Email)
                                     .Replace("{{Subject}}", model.Subject)
                                     .Replace("{{Message}}", model.Message);

        // Send email
        await _emailService.SendEmailAsync(model.Email, model.Subject, emailTemplate);

        return RedirectToAction("ThankYou");
    }

    public IActionResult Chitiet()
    {
        return View();
    }
    public IActionResult About()
{
    ViewData["IsFullWidth"] = true;
    return View();
}




}
