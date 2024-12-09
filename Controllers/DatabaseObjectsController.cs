using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/database/objects")]
    [Authorize(Roles = "Administrator")]
    public class DatabaseObjectsController : ControllerBase
    {
        private readonly IDatabaseObjectsService _databaseObjectsService;

        public DatabaseObjectsController(IDatabaseObjectsService databaseObjectsService)
        {
            _databaseObjectsService = databaseObjectsService;
        }

        // GET: api/database/objects/tables
        [HttpGet("tables")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllTables()
        {
            var tables = await _databaseObjectsService.GetAllTablesAsync();
            return Ok(tables);
        }

        // GET: api/database/objects/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllDatabaseObjects()
        {
            var objects = await _databaseObjectsService.GetAllDatabaseObjectsAsync();
            return Ok(objects);
        }

        // GET: api/database/objects/tables/{tableName}/columns
        [HttpGet("tables/{tableName}/columns")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetTableColumns(string tableName)
        {
            var columns = await _databaseObjectsService.GetTableColumnsAsync(tableName);
            return Ok(columns);
        }

        // GET: api/database/objects/tables/{tableName}/data
        [HttpGet("tables/{tableName}/data")]
        public async Task<ActionResult<object>> GetTableData(
            string tableName,
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0)
        {
            var (data, totalCount) = await _databaseObjectsService.GetTableDataAsync(tableName, limit, offset);
            return Ok(new { data, totalCount });
        }

        // PUT: api/database/objects/tables/{tableName}
        [HttpPut("tables/{tableName}")]
        public async Task<ActionResult<string>> SaveTableData(string tableName, [FromBody] Dictionary<string, object> data)
        {
            var result = await _databaseObjectsService.SaveTableDataAsync(tableName, data);
            return Ok(result);
        }
    }
}
