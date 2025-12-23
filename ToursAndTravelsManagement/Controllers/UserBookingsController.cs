using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.Services.EmailService;
using ToursAndTravelsManagement.Services.PdfService;
using Microsoft.EntityFrameworkCore;


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
        return NotFound();

    ViewBag.Tour = tour;

    // 🔥 LẤY USER
    var currentUser = await _userManager.GetUserAsync(User);

    if (currentUser?.MembershipTierId != null)
    {
        var tier = await _unitOfWork.MembershipTierRepository
            .GetByIdAsync(currentUser.MembershipTierId.Value);

        ViewBag.MembershipTier = tier;
    }

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
        var userId = _userManager.GetUserId(User);

var currentUser = await _userManager.Users
    .Include(u => u.MembershipTier)
    .FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser == null)
            return Unauthorized();
        
        // ===== GÁN DỮ LIỆU HỆ THỐNG =====
        booking.UserId = currentUser.Id;
        booking.BookingDate = DateTime.Now;
        booking.Status = BookingStatus.Pending;
        booking.PaymentStatus = PaymentStatus.Pending;
        booking.CreatedDate = DateTime.Now;
        booking.CreatedBy = currentUser.Email;
        booking.IsActive = true;

        var tour = await _unitOfWork.TourRepository.GetByIdAsync(booking.TourId);
        if (tour == null)
            return NotFound("Selected tour not found.");

        // ===== TÍNH GIÁ GỐC =====
        var totalPrice = tour.Price * booking.NumberOfParticipants;
        booking.TotalPrice = totalPrice;

decimal finalPrice = totalPrice;
booking.DiscountAmount = 0;

// ===== GIẢM THEO HẠNG =====
if (currentUser.MembershipTierId.HasValue)
{
    var tier = await _unitOfWork.MembershipTierRepository
        .GetByIdAsync(currentUser.MembershipTierId.Value);

    if (tier != null)
    {
        var tierDiscount = totalPrice * tier.DiscountPercent / 100;
        booking.DiscountAmount += tierDiscount;
        finalPrice -= tierDiscount;
    }
}

// ===== GIẢM THEO VOUCHER (NẾU CÓ) =====
if (booking.VoucherId.HasValue)
{
    var voucher = await _unitOfWork.VoucherRepository
        .GetByIdAsync(booking.VoucherId.Value);

    if (voucher == null
        || !voucher.IsActive
        || DateTime.Now < voucher.StartDate
        || DateTime.Now > voucher.EndDate
        || voucher.Quantity <= 0)
    {
        ModelState.AddModelError("", "Voucher không hợp lệ hoặc đã hết hạn.");
        ViewBag.Tour = tour;
        return View(booking);
    }

    decimal voucherDiscount = 0;

    if (voucher.IsPercentage)
    {
        // Giảm theo %
        voucherDiscount = finalPrice * voucher.DiscountValue / 100;

        // Áp trần nếu có
        if (voucher.MaxDiscountAmount.HasValue &&
            voucherDiscount > voucher.MaxDiscountAmount.Value)
        {
            voucherDiscount = voucher.MaxDiscountAmount.Value;
        }
    }
    else
    {
        // Giảm tiền cố định
        voucherDiscount = voucher.DiscountValue;
    }

    booking.DiscountAmount += voucherDiscount;
    finalPrice -= voucherDiscount;

    // Giảm số lượt dùng
    voucher.Quantity -= 1;
    _unitOfWork.VoucherRepository.Update(voucher);
}


booking.FinalPrice = Math.Max(0, finalPrice);

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

                // ===== CỘNG DOANH THU + XÉT HẠNG =====
if (booking.PaymentStatus == PaymentStatus.Completed)
{
    currentUser.TotalRevenue += booking.FinalPrice;

    await UpdateMembershipTier(currentUser);

    await _userManager.UpdateAsync(currentUser);
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
var userId = _userManager.GetUserId(User);

var currentUser = await _userManager.Users
    .Include(u => u.MembershipTier)
    .FirstOrDefaultAsync(u => u.Id == userId);


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
var userId = _userManager.GetUserId(User);

var currentUser = await _userManager.Users
    .Include(u => u.MembershipTier)
    .FirstOrDefaultAsync(u => u.Id == userId);

if (currentUser == null)
    return Unauthorized();

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

    private async Task UpdateMembershipTier(ApplicationUser user)
{
    var tiers = await _unitOfWork.MembershipTierRepository.GetAllAsync();

    var matchedTier = tiers
        .OrderByDescending(t => t.MinRevenue)
        .FirstOrDefault(t => user.TotalRevenue >= t.MinRevenue);

    if (matchedTier != null)
    {
        user.MembershipTierId = matchedTier.Id;
    }
}

}
