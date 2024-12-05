namespace RoomReservationSystem.Models
{
    public class RoomEquipment
    {
        public int RoomId { get; set; }
        public int EquipmentId { get; set; }
        
        public Room Room { get; set; }
        public Equipment Equipment { get; set; }
    }
}
