using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace RoomReservationSystem.Models
{
    public class RoomFilterParameters
    {
        public string? Name { get; set; }
        
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }
        
        public List<int>? EquipmentIds { get; set; }
        
        public int? BuildingId { get; set; }
    }
}
