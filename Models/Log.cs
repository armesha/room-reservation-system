using System;

namespace RoomReservationSystem.Models
{
    public class Log
    {
        public int LogId { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public DateTime LogDate { get; set; }
        public string TableName { get; set; }
    }
}
