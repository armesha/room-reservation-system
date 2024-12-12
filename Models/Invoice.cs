using System;

namespace RoomReservationSystem.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public int BookingId { get; set; }  
        public decimal Amount { get; set; }
        public int isPaid { get; set; } 
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public int RoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public string Username { get; set; }
        public string RoomNumber { get; set; }
        public string BuildingName { get; set; }
    }
}
