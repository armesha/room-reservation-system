using System;

namespace RoomReservationSystem.Models
{
    public class EventBookingDetail : Event
    {
        public int? RoomId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
    }
}
