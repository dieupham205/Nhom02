using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToursAndTravelsManagement.Models;

namespace ToursAndTravelsManagement.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<TourItinerary> TourItineraries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Additional configurations if needed
    }
}