// Models/User.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RoomReservationSystem.Models
{
    public class User
    {
        public int UserId { get; set; }

        public required string Username { get; set; }

        [JsonIgnore]
        public required string PasswordHash { get; set; }

        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public int RoleId { get; set; }

        [JsonIgnore] // This will be set from RoleName
        public string RoleName { get; set; }

        public DateTime RegistrationDate { get; set; }
    }
}
