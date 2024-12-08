using RoomReservationSystem.Models;
using RoomReservationSystem.Models.Auth;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public interface IUserService
    {
        RegisterResponse Register(RegisterRequest request);
        RegisterResponse AdminCreateUser(AdminUserCreateRequest request);
        LoginResponse Authenticate(LoginRequest request);
        User GetUserById(int userId);
        User GetUserByUsername(string username);
        User UpdateUser(int userId, UpdateUserRequest request);
        bool ChangePassword(int userId, string currentPassword, string newPassword);
        void DeleteUser(int userId);
        void AddUser(User user);
        IEnumerable<User> GetPaginatedUsers(UserFilterParameters parameters);
        int GetTotalUsersCount();
    }
}
