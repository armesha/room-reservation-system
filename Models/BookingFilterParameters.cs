using System;

namespace RoomReservationSystem.Models
{
    public class BookingFilterParameters
    {
        public int? UserId { get; set; }
        public int? RoomId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public bool? HasEvent { get; set; }
    }
}
