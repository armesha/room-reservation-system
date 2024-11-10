// Services/BookingService.cs
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public IEnumerable<Booking> GetAllBookingsForAdmin()
        {
            return _bookingRepository.GetAllBookings();
        }

        public IEnumerable<Booking> GetAllBookingsForUser(int userId)
        {
            return _bookingRepository.GetBookingsByUserId(userId);
        }

        public Booking GetBookingById(int bookingId)
        {
            return _bookingRepository.GetBookingById(bookingId);
        }

        public void AddBooking(Booking booking)
        {
            _bookingRepository.AddBooking(booking);
        }

        public void UpdateBooking(Booking booking)
        {
            _bookingRepository.UpdateBooking(booking);
        }

        public void DeleteBooking(int bookingId)
        {
            _bookingRepository.DeleteBooking(bookingId);
        }
    }
}
