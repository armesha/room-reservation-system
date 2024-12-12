using System;
using System.Text.Json.Serialization;

namespace RoomReservationSystem.Models
{
    public class DailyBookingSummary
    {
        [JsonPropertyName("day")]
        public DateTime Day { get; set; }

        [JsonPropertyName("booking_count")]
        public int BookingCount { get; set; }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }
    }
}
