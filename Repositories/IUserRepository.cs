using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface IUserRepository
    {
        User GetUserByUsername(string username);
        User GetUserByEmail(string email);
        User GetUserById(int userId);
        IEnumerable<User> GetAllUsers();
        IEnumerable<User> GetPaginatedUsers(UserFilterParameters parameters);
        int GetTotalUsersCount();
        void AddUser(User user);
        User UpdateUser(User user);
        void DeleteUser(int userId);
    }
}
