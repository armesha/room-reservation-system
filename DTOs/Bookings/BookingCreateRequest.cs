// DTOs/Bookings/BookingCreateRequest.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.DTOs.Bookings
{
    public class BookingCreateRequest
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Optional: Add additional properties as needed
    }
}
