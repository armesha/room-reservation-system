using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoomReservationSystem.Repositories
{
    public interface IDatabaseObjectsRepository
    {
        Task<IEnumerable<string>> GetAllTablesAsync();
        Task<IEnumerable<string>> GetAllDatabaseObjectsAsync();
        Task<IEnumerable<dynamic>> GetTableColumnsAsync(string tableName);
        Task<(IEnumerable<dynamic> Data, int TotalCount)> GetTableDataAsync(string tableName, int limit = 10, int offset = 0);
        Task<string> SaveTableDataAsync(string tableName, Dictionary<string, object> data);
    }
}
