// Repositories/IBookingRepository.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface IBookingRepository
    {
        Booking GetBookingById(int bookingId);
        (IEnumerable<Booking> Bookings, int TotalCount) GetAllBookings(int? limit = null, int? offset = null, BookingFilterParameters filters = null);
        (IEnumerable<Booking> Bookings, int TotalCount) GetBookingsByUserId(int userId, int? limit = null, int? offset = null, BookingFilterParameters filters = null);
        void AddBooking(Booking booking);
        void UpdateBooking(Booking booking);
        void DeleteBooking(int bookingId);
        IEnumerable<Invoice> GetUserInvoices(int userId);
        IEnumerable<Invoice> GetUnpaidInvoices();
        IEnumerable<Invoice> GetPaidInvoices();
        bool MarkInvoiceAsPaid(int invoiceId);
    }
}
