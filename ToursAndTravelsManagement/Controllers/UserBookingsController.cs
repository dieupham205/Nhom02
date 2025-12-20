using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.Services.EmailService;
using ToursAndTravelsManagement.Services.PdfService;


namespace ToursAndTravelsManagement.Controllers;


[Authorize(Policy = "RequireUserRole")] // Chỉ User role mới được vào
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

    // =========================
    // GET: UserBookings/AvailableTours
    // =========================
    [HttpGet]
    public async Task<IActionResult> AvailableTours()
    {
        var tours = await _unitOfWork.TourRepository.GetAllAsync();
        return View(tours);
    }

    // =========================
    // GET: UserBookings/BookTour/{id}
    // =========================
    [HttpGet]
    public async Task<IActionResult> BookTour(int id)
    {
        var tour = (await _unitOfWork.TourRepository.GetAllAsync(
            t => t.TourId == id,
            includeProperties: "Destination"
        )).FirstOrDefault();

        if (tour == null)
        {
            return NotFound();
        }

        ViewBag.Tour = tour;

        var booking = new Booking
        {
            TourId = tour.TourId,
            BookingDate = DateTime.Now
        };

        return View(booking);
    }

    // =========================
    // POST: UserBookings/BookTour
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookTour(Booking booking)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return Unauthorized();

        var tour = await _unitOfWork.TourRepository.GetByIdAsync(booking.TourId);
        if (tour == null)
            return NotFound("Selected tour not found.");

        // ===== TÍNH GIÁ GỐC =====
        var totalPrice = tour.Price * booking.NumberOfParticipants;
        booking.TotalPrice = totalPrice;

        // ===== XỬ LÝ VOUCHER (NẾU CÓ) =====
        if (booking.VoucherId.HasValue && booking.DiscountAmount > 0)
        {
            var voucher = await _unitOfWork.VoucherRepository
                .GetByIdAsync(booking.VoucherId.Value);

            if (voucher == null || !voucher.IsActive)
            {
                ModelState.AddModelError("", "Voucher không hợp lệ.");
                ViewBag.Tour = tour;
                return View(booking);
            }

            // Giảm tiền
            booking.FinalPrice = Math.Max(0, totalPrice - booking.DiscountAmount);

            // Gắn voucher
            booking.VoucherId = voucher.VoucherId;
        }
        else
        {
            booking.FinalPrice = totalPrice;
        }

        // ===== GÁN DỮ LIỆU HỆ THỐNG =====
        booking.UserId = currentUser.Id;
        booking.BookingDate = DateTime.Now;
        booking.Status = BookingStatus.Pending;
        booking.PaymentStatus = PaymentStatus.Pending;
        booking.IsActive = true;

        // ===== THANH TOÁN (MOCK) =====
        switch (booking.PaymentMethod)
        {
            case PaymentMethod.CreditCard:
            case PaymentMethod.EWallet:
                booking.PaymentStatus = PaymentStatus.Completed;
                booking.Status = BookingStatus.Confirmed;
                break;

            case PaymentMethod.BankTransfer:
                booking.PaymentStatus = PaymentStatus.Pending;
                booking.Status = BookingStatus.Pending;
                break;
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Tour = tour;
            return View(booking);
        }

        // ===== SAVE BOOKING =====
        await _unitOfWork.BookingRepository.AddAsync(booking);
        await _unitOfWork.CompleteAsync();

        // ===== GENERATE TICKET =====
        var ticket = new Ticket
        {
            TicketNumber = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            CustomerName = currentUser.UserName,
            TourName = tour.Name,
            BookingDate = booking.BookingDate,
            TourStartDate = tour.StartDate,
            TourEndDate = tour.EndDate,
            TotalPrice = booking.FinalPrice
        };

        var pdf = _pdfService.GenerateTicketPdf(ticket);

        await _emailService.SendTicketEmailAsync(
            currentUser.Email,
            $"Your Ticket - {ticket.TicketNumber}",
            "Thank you for booking! Please find your ticket attached.",
            pdf
        );

        return RedirectToAction(nameof(Success));
    }

    // =========================
    // GET: UserBookings/Success
    // =========================
    [HttpGet]
    public IActionResult Success()
    {
        return View();
    }

    // =========================
    // GET: UserBookings/MyBookings
    // =========================
    [HttpGet]
    public async Task<IActionResult> MyBookings(bool full = false)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var bookingsQuery = (await _unitOfWork.BookingRepository.GetAllAsync(
            b => b.UserId == currentUser.Id,
            includeProperties: "Tour.Destination"
        ))
        .OrderByDescending(b => b.BookingDate);

        var vm = new UserMyBookingsViewModel
        {
            User = currentUser,
            Bookings = full
                ? bookingsQuery.ToList()      // FULL lịch sử
                : bookingsQuery.Take(3).ToList() // PREVIEW: 3 booking gần nhất
        };

        ViewBag.IsFullHistory = full;

        return View(vm);
    }

    // =========================
    // POST: UserBookings/CancelBooking
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int bookingId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);
        if (booking == null || booking.UserId != currentUser.Id)
        {
            return NotFound();
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return BadRequest("Booking already cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.IsActive = false;
        _unitOfWork.BookingRepository.Update(booking);
        await _unitOfWork.CompleteAsync();
        return RedirectToAction(nameof(MyBookings));
    }

    // =========================
    // GET: UserBookings/ExportBookingsPdf
    // =========================
    [HttpGet]
    public async Task<IActionResult> ExportBookingsPdf()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }
        Log.Information("User {User} exporting bookings PDF", currentUser.UserName);

        var bookings = await _unitOfWork.BookingRepository.GetAllAsync(
            b => b.UserId == currentUser.Id,
            includeProperties: "Tour"
        );

        if (!bookings.Any())
        {
            return NotFound("No bookings found.");
        }

        var pdf = _pdfService.GenerateBookingsPdf(bookings.ToList());

        return File(pdf, "application/pdf", "MyBookings.pdf");
    }

    // =========================
    // GET: UserBookings/History
    // =========================
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
            return Unauthorized();

        var bookings = await _unitOfWork.BookingRepository.GetAllAsync(
            b => b.UserId == currentUser.Id,
            includeProperties: "Tour.Destination"
        );

        var vm = new UserMyBookingsViewModel
        {
            User = currentUser,
            Bookings = bookings
        };

        return View(vm);
    }
}
