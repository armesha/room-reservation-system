//Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User"; // Roles: User, Admin

        public bool IsActive { get; set; } = true;

        // Additional properties like PhoneNumber, Address can be added here
    }
}
