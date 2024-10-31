// DTOs/Auth/RegisterRequest.cs (Optional Update)
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.DTOs.Auth
{
    public class RegisterRequest
    {
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
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Optional: Add Address information
        // public AddressCreateRequest Address { get; set; } = null!;
    }
}
