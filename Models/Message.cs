using System;

namespace RoomReservationSystem.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int? SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; }
    }
}
