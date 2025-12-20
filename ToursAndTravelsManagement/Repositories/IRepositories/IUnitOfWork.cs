using ToursAndTravelsManagement.Models;

namespace ToursAndTravelsManagement.Repositories.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Booking> BookingRepository { get; }
    IGenericRepository<Tour> TourRepository { get; }
    IGenericRepository<Destination> DestinationRepository { get; }
    IGenericRepository<TourItinerary> TourItineraryRepository { get; }
    Task CompleteAsync();
}
