using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using System.Collections.Generic;

namespace RoomReservationSystem.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class BuildingsController : ControllerBase
    {
        private readonly IBuildingService _buildingService;
        private readonly ILogger<BuildingsController> _logger;

        public BuildingsController(IBuildingService buildingService, ILogger<BuildingsController> logger)
        {
            _buildingService = buildingService;
            _logger = logger;
        }

        // GET: /api/admin/buildings
        [HttpGet]
        public ActionResult<IEnumerable<Building>> GetAllBuildings()
        {
            _logger.LogInformation("Fetching all buildings.");
            var buildings = _buildingService.GetAllBuildings();
            return Ok(buildings);
        }

        // GET: /api/admin/buildings/{id}
        [HttpGet("{id}")]
        public ActionResult<Building> GetBuildingById(int id)
        {
            _logger.LogInformation($"Fetching building with ID: {id}");
            var building = _buildingService.GetBuildingById(id);
            if (building == null)
            {
                _logger.LogWarning($"Building with ID: {id} not found.");
                return NotFound(new { message = "Building not found." });
            }

            return Ok(building);
        }

        // POST: /api/admin/buildings
        [HttpPost]
        public IActionResult AddBuilding([FromBody] Building building)
        {
            _logger.LogInformation("Adding a new building.");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for adding a building.");
                return BadRequest(ModelState);
            }

            _buildingService.AddBuilding(building);
            _logger.LogInformation($"Building added with ID: {building.BuildingId}");
            return CreatedAtAction(nameof(GetBuildingById), new { id = building.BuildingId }, building);
        }

        // PUT: /api/admin/buildings/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateBuilding(int id, [FromBody] Building building)
        {
            _logger.LogInformation($"Updating building with ID: {id}");
            if (id != building.BuildingId)
            {
                _logger.LogWarning("ID mismatch in update request.");
                return BadRequest(new { message = "ID mismatch." });
            }

            var existingBuilding = _buildingService.GetBuildingById(id);
            if (existingBuilding == null)
            {
                _logger.LogWarning($"Building with ID: {id} not found for update.");
                return NotFound(new { message = "Building not found." });
            }

            _buildingService.UpdateBuilding(building);
            _logger.LogInformation($"Building with ID: {id} updated successfully.");
            return NoContent();
        }

        // DELETE: /api/admin/buildings/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteBuilding(int id)
        {
            _logger.LogInformation($"Attempting to delete building with ID: {id}");
            var existingBuilding = _buildingService.GetBuildingById(id);
            if (existingBuilding == null)
            {
                _logger.LogWarning($"Building with ID: {id} not found for deletion.");
                return NotFound(new { message = "Building not found." });
            }

            _buildingService.DeleteBuilding(id);
            _logger.LogInformation($"Building with ID: {id} deleted successfully.");
            return NoContent();
        }
    }
}
