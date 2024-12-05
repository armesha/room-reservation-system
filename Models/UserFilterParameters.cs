using System;

namespace RoomReservationSystem.Models
{
    public class UserFilterParameters
    {
        public int Offset { get; set; } = 0;
        public int Count { get; set; } = 10;
    }
}
