using System;
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string? Status { get; set; } = "Pending";

        public bool HasEvent { get; set; }

        public string? Username { get; set; }
    }
}