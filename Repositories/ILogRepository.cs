using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface ILogRepository
    {
        IEnumerable<Log> GetAllLogs();
        IEnumerable<Log> GetLogsByUsername(string username);
        Log GetLogById(int logId);
    }
}
