//DTOs/Auth/LoginRequest.cs
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
