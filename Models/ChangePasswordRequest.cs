using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
