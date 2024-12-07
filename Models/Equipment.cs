using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RoomReservationSystem.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
