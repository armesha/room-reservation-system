// DTOs/Rooms/RoomResponse.cs
namespace RoomReservationSystem.DTOs.Rooms
{
    public class RoomResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }

        // Additional properties like Price, Type can be added here
    }
}
