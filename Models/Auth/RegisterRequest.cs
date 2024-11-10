using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models.Auth
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } // Plain text; will be hashed

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}