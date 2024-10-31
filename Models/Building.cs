// Models/Building.cs
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class Building
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Acronym { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NumberOfFloors { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
