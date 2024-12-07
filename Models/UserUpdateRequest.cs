using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class UserUpdateRequest
    {
        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? RoleName { get; set; }

        [StringLength(50)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Surname { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(2)]
        public string? Code { get; set; }
    }
}
