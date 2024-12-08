namespace RoomReservationSystem.Models
{
    public class OptimalRoom
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public int EquipmentMatchCount { get; set; }
        public decimal TotalScore { get; set; }
    }

    public class RoomOccupancyData
    {
        public DateTime SlotDate { get; set; }
        public int BookingsCount { get; set; }
        public decimal OccupancyPercentage { get; set; }
        public decimal MovingAverageBookings { get; set; }
        public string DayType { get; set; }
    }
}
