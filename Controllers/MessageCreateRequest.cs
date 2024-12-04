// Models/MessageCreateRequest.cs
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.Models
{
    public class MessageCreateRequest
    {
        [Required]
        public string ReceiverUsername { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }
    }
}
