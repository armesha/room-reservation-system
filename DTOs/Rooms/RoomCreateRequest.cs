// DTOs/Rooms/RoomCreateRequest.cs (Updated)
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.DTOs.Rooms
{
    public class RoomCreateRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int BuildingId { get; set; }

        // Additional properties like Price, Type can be added here
    }
}
