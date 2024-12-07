using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class AdminUserCreateRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
