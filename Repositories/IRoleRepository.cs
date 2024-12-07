using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface IRoleRepository
    {
        Role GetRoleById(int roleId);
        Role GetRoleByName(string roleName);
        IEnumerable<Role> GetAllRoles();
    }
}
