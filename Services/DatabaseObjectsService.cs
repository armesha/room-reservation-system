using RoomReservationSystem.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoomReservationSystem.Services
{
    public class DatabaseObjectsService : IDatabaseObjectsService
    {
        private readonly IDatabaseObjectsRepository _repository;

        public DatabaseObjectsService(IDatabaseObjectsRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<string>> GetAllTablesAsync()
        {
            return await _repository.GetAllTablesAsync();
        }

        public async Task<IEnumerable<string>> GetAllDatabaseObjectsAsync()
        {
            return await _repository.GetAllDatabaseObjectsAsync();
        }

        public async Task<IEnumerable<dynamic>> GetTableColumnsAsync(string tableName)
        {
            return await _repository.GetTableColumnsAsync(tableName);
        }

        public async Task<(IEnumerable<dynamic> Data, int TotalCount)> GetTableDataAsync(string tableName, int limit = 10, int offset = 0)
        {
            return await _repository.GetTableDataAsync(tableName, limit, offset);
        }

        public async Task<string> SaveTableDataAsync(string tableName, Dictionary<string, object> data)
        {
            return await _repository.SaveTableDataAsync(tableName, data);
        }
    }
}
