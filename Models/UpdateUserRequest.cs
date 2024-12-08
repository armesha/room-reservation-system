using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class UpdateUserRequest
    {
        public string Username { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public int? RoleId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Code { get; set; }
    }
}
