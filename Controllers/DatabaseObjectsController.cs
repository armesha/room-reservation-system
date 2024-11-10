using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Services;
using System.Collections.Generic;

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

        // GET: /api/database/objects
        [HttpGet]
        public ActionResult<IEnumerable<string>> GetAllDatabaseObjects()
        {
            var objects = _databaseObjectsService.GetAllDatabaseObjects();
            return Ok(objects);
        }

        // GET: /api/database/objects/{name}
        [HttpGet("{name}")]
        public ActionResult<IEnumerable<string>> GetDatabaseObjectsByName(string name)
        {
            var objects = _databaseObjectsService.GetDatabaseObjectsByName(name);
            return Ok(objects);
        }
    }
}
