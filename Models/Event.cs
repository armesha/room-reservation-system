using System;

namespace RoomReservationSystem.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Description { get; set; }
        public int? BookingId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
