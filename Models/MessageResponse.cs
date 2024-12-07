using System;

namespace RoomReservationSystem.Models
{
    public class MessageResponse
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; }
        public string SenderFullInfo { get; set; }  // "Username (Name Surname)"
        public string ReceiverFullInfo { get; set; }  // "Username (Name Surname)"
    }
}
