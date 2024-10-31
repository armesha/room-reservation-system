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

        // Additional fields like Preferences can be added here
    }
}