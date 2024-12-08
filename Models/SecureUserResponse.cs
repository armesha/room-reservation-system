using System;
using System.Text.Json.Serialization;

namespace RoomReservationSystem.Models
{
    public class SecureUserResponse
    {
        // Basic information - always visible
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }

        // Properties visible only to administrators
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Email { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Name { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Surname { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Phone { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Country { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime? RegistrationDate { get; set; }

        public static SecureUserResponse FromUser(User user, string requestingUserRole, string userRoleName)
        {
            var response = new SecureUserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = userRoleName
            };

            // Only administrators can see sensitive information
            if (requestingUserRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                // For admin, set all fields regardless of whether they are null
                response.Email = user.Email;
                response.Name = user.Name;
                response.Surname = user.Surname;
                response.Phone = user.Phone;
                response.Country = user.Code;
                response.RegistrationDate = user.RegistrationDate;
            }

            return response;
        }
    }
}
