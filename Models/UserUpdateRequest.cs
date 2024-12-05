// Models/UserUpdateRequest.cs
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class UserUpdateRequest
    {
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string RoleName { get; set; } // Optional: To change the user's role
    }
}
