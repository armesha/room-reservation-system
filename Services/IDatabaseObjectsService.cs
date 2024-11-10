// Services/IDatabaseObjectsService.cs
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public interface IDatabaseObjectsService
    {
        IEnumerable<string> GetAllDatabaseObjects();
        IEnumerable<string> GetDatabaseObjectsByName(string name);
    }
}
