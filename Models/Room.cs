
namespace RoomReservationSystem.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public int BuildingId { get; set; }
        public string RoomNumber { get; set; }
        public int Capacity { get; set; }
        public bool HasProjector { get; set; }
        public bool HasWhiteboard { get; set; }
    }
}
