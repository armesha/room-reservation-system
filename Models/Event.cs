using System;

namespace RoomReservationSystem.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Description { get; set; }
        public int? BookingId { get; set; }  // Optional reference to booking
        public int CreatedBy { get; set; }  // Reference to the user who created the event
        public DateTime CreatedAt { get; set; }
        public int? ParentEventId { get; set; }  // Reference to parent event
        public int HierarchyLevel { get; set; }  // Level in the hierarchy
        public bool IsLeaf { get; set; }  // Indicates if this is a leaf node
        public string EventPath { get; set; }  // Full path of event names from root to this event
    }
}
