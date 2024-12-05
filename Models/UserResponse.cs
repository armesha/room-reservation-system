// Models/UserResponse.cs
namespace RoomReservationSystem.Models
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
