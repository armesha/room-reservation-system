using System.Collections.Generic;

namespace RoomReservationSystem.Models
{
    public class RoomUpdateRequest
    {
        public int RoomId { get; set; }
        public int BuildingId { get; set; }
        public string RoomNumber { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? IdFile { get; set; }
        public List<EquipmentReference> Equipment { get; set; }
    }

    public class EquipmentReference
    {
        public int EquipmentId { get; set; }
    }
}
