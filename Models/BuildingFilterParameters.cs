namespace RoomReservationSystem.Models
{
    public class BuildingFilterParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;
        
        public int Offset { get; set; } = 0;
        
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
        
        public string? BuildingName { get; set; }
        public string? Address { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; }
    }
}
