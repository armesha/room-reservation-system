using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;

        public BookingService(IBookingRepository bookingRepository, 
            IRoomRepository roomRepository)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
        }

        public (IEnumerable<Booking> Bookings, int TotalCount) GetAllBookingsForAdmin(int? limit = null, int? offset = null, BookingFilterParameters filters = null)
        {
            return _bookingRepository.GetAllBookings(limit, offset, filters);
        }

        public (IEnumerable<Booking> Bookings, int TotalCount) GetAllBookingsForUser(int userId, int? limit = null, int? offset = null, BookingFilterParameters filters = null)
        {
            if (filters == null)
            {
                filters = new BookingFilterParameters();
            }
            filters.UserId = userId;
            return _bookingRepository.GetAllBookings(limit, offset, filters);
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

        public IEnumerable<Invoice> GetUserInvoices(int userId)
        {
            return _bookingRepository.GetUserInvoices(userId);
        }

        public IEnumerable<Invoice> GetUnpaidInvoices()
        {
            return _bookingRepository.GetUnpaidInvoices();
        }

        public IEnumerable<Invoice> GetPaidInvoices()
        {
            return _bookingRepository.GetPaidInvoices();
        }

        public bool MarkInvoiceAsPaid(int invoiceId)
        {
            return _bookingRepository.MarkInvoiceAsPaid(invoiceId);
        }

        public async Task<IEnumerable<DailyBookingSummary>> GetDailyBookingSummaryAsync()
        {
            return await _bookingRepository.GetDailyBookingSummaryAsync();
        }
    }
}
