// Services/IBookingService.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public interface IBookingService
    {
        IEnumerable<Booking> GetAllBookingsForAdmin();
        IEnumerable<Booking> GetAllBookingsForUser(int userId);
        Booking GetBookingById(int bookingId);
        void AddBooking(Booking booking);
        void UpdateBooking(Booking booking);
        void DeleteBooking(int bookingId);
    }
}
