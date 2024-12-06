using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface IUserRepository
    {
        User GetUserByUsername(string username);
        User GetUserById(int userId);
        IEnumerable<User> GetAllUsers();
        IEnumerable<User> GetPaginatedUsers(UserFilterParameters parameters);
        int GetTotalUsersCount();
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(int userId);
    }
}
