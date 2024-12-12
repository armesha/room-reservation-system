using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models.Auth
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        /// <summary>
        /// Indicates if this login request is for user emulation
        /// </summary>
        public bool IsEmulation { get; set; }
    }
}