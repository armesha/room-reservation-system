using System;
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class Booking
    {
        public Booking()
        {
            Status = "Pending";
        }

        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string Status { get; set; }
    }
}
