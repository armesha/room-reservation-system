using System;

namespace RoomReservationSystem.Models
{
    public class AdminUserResponse : BasicUserResponse
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public DateTime RegistrationDate { get; set; }

        public static AdminUserResponse FromUser(User user, string roleName)
        {
            return new AdminUserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = roleName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                Phone = user.Phone,
                Country = user.Code,
                RegistrationDate = user.RegistrationDate
            };
        }
    }
}
