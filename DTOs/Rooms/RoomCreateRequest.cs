//DTOs/Rooms/RoomCreateRequest.cs
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.DTOs.Rooms
{
    public class RoomCreateRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        // Additional properties like Price, Type can be added here
    }
}
