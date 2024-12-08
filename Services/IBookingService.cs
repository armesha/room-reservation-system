using RoomReservationSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoomReservationSystem.Services
{
    public interface IBookingService
    {
        Booking GetBookingById(int bookingId);
        (IEnumerable<Booking> Bookings, int TotalCount) GetAllBookingsForAdmin(int? limit = null, int? offset = null, BookingFilterParameters filters = null);
        (IEnumerable<Booking> Bookings, int TotalCount) GetAllBookingsForUser(int userId, int? limit = null, int? offset = null, BookingFilterParameters filters = null);
        void AddBooking(Booking booking);
        void UpdateBooking(Booking booking);
        void DeleteBooking(int bookingId);
        IEnumerable<Invoice> GetUserInvoices(int userId);
        IEnumerable<Invoice> GetUnpaidInvoices();
        IEnumerable<Invoice> GetPaidInvoices();
        bool MarkInvoiceAsPaid(int invoiceId);
        Task<IEnumerable<DailyBookingSummary>> GetDailyBookingSummaryAsync();
    }
}
