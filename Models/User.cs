using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User"; // Roles: User, Admin

        [Required]
        [Column("IS_ACTIVE")]
        [MaxLength(1)]
        public string IsActive { get; set; } = "Y"; // 'Y' for active, 'N' for inactive

        [NotMapped]
        public bool IsActiveBool
        {
            get => IsActive == "Y";
            set => IsActive = value ? "Y" : "N";
        }

        // Additional properties like PhoneNumber, Address can be added here
    }
}
