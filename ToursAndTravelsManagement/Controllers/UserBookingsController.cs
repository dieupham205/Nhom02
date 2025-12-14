using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.Services.EmailService;
using ToursAndTravelsManagement.Services.PdfService;
using System.Threading.Tasks;

namespace ToursAndTravelsManagement.Controllers;

// CHỐT: [Authorize] -> Ai đăng nhập rồi cũng vào được (không cần Role Customer)
[Authorize] 
public class UserBookingsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;

    public UserBookingsController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IPdfService pdfService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
        _pdfService = pdfService;
    }

    // Trang này ai cũng xem được tour (kể cả chưa login)
    [AllowAnonymous] 
    public async Task<IActionResult> AvailableTours()
    {
        var tours = await _unitOfWork.TourRepository.GetAllAsync();
        return View(tours);
    }

    // Các hàm dưới này cần đăng nhập
    public async Task<IActionResult> BookTour(int? id)
    {
        if (id == null) return NotFound();
        var tour = await _unitOfWork.TourRepository.GetByIdAsync(id.Value);
        if (tour == null) return NotFound();
        ViewBag.Tour = tour;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookTour([Bind("TourId,BookingDate,NumberOfParticipants,PaymentMethod")] Booking booking)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        booking.UserId = currentUser.Id;
        var tour = await _unitOfWork.TourRepository.GetByIdAsync(booking.TourId);
        if (tour == null) return NotFound("Selected tour not found.");

        booking.TotalPrice = tour.Price * booking.NumberOfParticipants;

        if (ModelState.IsValid)
        {
            await _unitOfWork.BookingRepository.AddAsync(booking);
            await _unitOfWork.CompleteAsync();

            var ticket = new Ticket
            {
                TicketNumber = Guid.NewGuid().ToString().Substring(0, 8),
                CustomerName = currentUser.UserName,
                TourName = tour.Name,
                BookingDate = DateTime.Now,
                TourStartDate = tour.StartDate,
                TourEndDate = tour.EndDate,
                TotalPrice = booking.TotalPrice
            };

            var pdf = _pdfService.GenerateTicketPdf(ticket);
            await _emailService.SendTicketEmailAsync(currentUser.Email, $"Your Ticket - {ticket.TicketNumber}", "Thank you for booking! Please find your ticket attached.", pdf);

            return RedirectToAction("MyBookings");
        }
        return View(booking);
    }

    // ĐÂY SẼ LÀ TRANG CHÍNH "THÔNG TIN KHÁCH HÀNG" CỦA BẠN
    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var bookings = await _unitOfWork.BookingRepository.GetAllAsync(
            b => b.UserId == currentUser.Id,
            includeProperties: "Tour"
        );

        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MyBookings(int bookingId, string action)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        if (action == "Cancel")
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);
            if (booking == null || booking.UserId != currentUser.Id) return NotFound();

            if (booking.Status == BookingStatus.Canceled) return BadRequest("Booking is already canceled.");

            booking.Status = BookingStatus.Canceled;
            booking.IsActive = false;

            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("MyBookings");
        }
        return BadRequest("Invalid action.");
    }

    [HttpGet]
    public async Task<IActionResult> ExportBookingsPdf()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var bookings = await _unitOfWork.BookingRepository.GetAllAsync(
            b => b.UserId == currentUser.Id,
            includeProperties: "Tour"
        );

        if (bookings == null || !bookings.Any()) return NotFound("No bookings found to export.");

        var bookingsList = bookings.ToList();
        var pdf = _pdfService.GenerateBookingsPdf(bookingsList);
        return File(pdf, "application/pdf", "BookingsReport.pdf");
    }
}