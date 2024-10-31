// DTOs/Addresses/AddressResponse.cs
namespace RoomReservationSystem.DTOs.Addresses
{
    public class AddressResponse
    {
        public int Id { get; set; }
        public string? Street { get; set; }
        public string City { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string? OrientationNumber { get; set; }
        public string? ApartmentNumber { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int BuildingId { get; set; }

        // Optional: Include building details if needed
        // public BuildingResponse Building { get; set; } = null!;
    }
}
