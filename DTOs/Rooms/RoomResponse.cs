// DTOs/Rooms/RoomResponse.cs (Updated)
namespace RoomReservationSystem.DTOs.Rooms
{
    public class RoomResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Description { get; set; } = string.Empty;
        public int BuildingId { get; set; }

        // Optional: Include building details if needed
        // public BuildingResponse Building { get; set; } = null!;
    }
}
