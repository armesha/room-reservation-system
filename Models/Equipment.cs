using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RoomReservationSystem.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [JsonIgnore]
        public string Description { get; set; }
    }
}
