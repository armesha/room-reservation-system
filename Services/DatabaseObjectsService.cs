// Services/DatabaseObjectsService.cs
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public class DatabaseObjectsService : IDatabaseObjectsService
    {
        private readonly IDatabaseObjectsRepository _databaseObjectsRepository;

        public DatabaseObjectsService(IDatabaseObjectsRepository databaseObjectsRepository)
        {
            _databaseObjectsRepository = databaseObjectsRepository;
        }

        public IEnumerable<string> GetAllDatabaseObjects()
        {
            return _databaseObjectsRepository.GetAllDatabaseObjects();
        }

        public IEnumerable<string> GetDatabaseObjectsByName(string name)
        {
            return _databaseObjectsRepository.GetDatabaseObjectsByName(name);
        }
    }
}
