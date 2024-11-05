using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models.Auth
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}