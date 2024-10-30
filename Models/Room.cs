//Models/Room.cs
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        // Additional properties like Price, Type can be added here
    }
}
