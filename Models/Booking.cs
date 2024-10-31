using System;
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Additional properties like Status, TotalPrice can be added here

        // Navigation properties
        public Room Room { get; set; }
        public User User { get; set; }
    }
}