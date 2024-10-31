// DTOs/Bookings/BookingResponse.cs (Optional Update)
namespace RoomReservationSystem.DTOs.Bookings
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Optional: Include related room and user details
        // public RoomResponse Room { get; set; } = null!;
        // public UserResponse User { get; set; } = null!;
    }
}
