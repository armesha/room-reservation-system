using System;

namespace RoomReservationSystem.DTOs.Bookings
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Additional properties like Status, TotalPrice can be added here
    }
}