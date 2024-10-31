// DTOs/Addresses/AddressCreateRequest.cs
using System.ComponentModel.DataAnnotations;

namespace RoomReservationSystem.DTOs.Addresses
{
    public class AddressCreateRequest
    {
        [MaxLength(255)]
        public string? Street { get; set; }

        [Required]
        [MaxLength(255)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string HouseNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? OrientationNumber { get; set; }

        [MaxLength(255)]
        public string? ApartmentNumber { get; set; }

        [Required]
        [MaxLength(10)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Country { get; set; } = string.Empty;

        [Required]
        public int BuildingId { get; set; }
    }
}
