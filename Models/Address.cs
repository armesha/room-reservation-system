using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomReservationSystem.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Street { get; set; } = string.Empty;

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

        [ForeignKey("BuildingId")]
        public Building Building { get; set; } = null!;
    }
}
