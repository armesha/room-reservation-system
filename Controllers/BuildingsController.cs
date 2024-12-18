using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using System.Collections.Generic;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingsController : ControllerBase
    {
        private readonly IBuildingService _buildingService;

        public BuildingsController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        // GET: /api/buildings
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<object> GetAllBuildings(
            [FromQuery] int offset = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? buildingName = null,
            [FromQuery] string? address = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isDescending = false)
        {
            if (!User.Identity.IsAuthenticated && pageSize > 10)
            {
                pageSize = 10;
            }

            var filterParams = new BuildingFilterParameters
            {
                Offset = offset,
                PageSize = pageSize,
                BuildingName = buildingName,
                Address = address,
                SortBy = sortBy,
                IsDescending = isDescending
            };

            var (buildings, totalCount) = _buildingService.GetAllBuildings(filterParams);
            
            return Ok(new { 
                list = buildings,
                metadata = new
                {
                    totalCount,
                    pageSize = filterParams.PageSize,
                    offset = filterParams.Offset
                }
            });
        }

        // GET: /api/buildings/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<Building> GetBuildingById(int id)
        {
            var building = _buildingService.GetBuildingById(id);
            if (building == null)
            {
                return NotFound(new { message = "Building not found." });
            }

            return Ok(new { building });
        }

        // POST: /api/buildings
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult AddBuilding([FromBody] Building building)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _buildingService.AddBuilding(building);
            return CreatedAtAction(nameof(GetBuildingById), new { id = building.BuildingId }, new { building });
        }

        // PUT: /api/buildings/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult UpdateBuilding(int id, [FromBody] Building building)
        {
            var existingBuilding = _buildingService.GetBuildingById(id);
            if (existingBuilding == null)
            {
                return NotFound(new { message = "Building not found." });
            }

            building.BuildingId = id;
            _buildingService.UpdateBuilding(building);

            var updatedBuilding = _buildingService.GetBuildingById(id);
            return Ok(new { building = updatedBuilding });
        }

        // DELETE: /api/buildings/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteBuilding(int id)
        {
            try
            {
                var building = _buildingService.GetBuildingById(id);
                if (building == null)
                {
                    return NotFound(new { message = "Building not found." });
                }

                _buildingService.DeleteBuilding(id);
                return Ok(new { success = true, message = "Building deleted successfully." });
            }
            catch (OracleException ex)
            {
                if (ex.Number == 2292) // ORA-02292: integrity constraint violation
                {
                    return BadRequest(new { message = "Cannot delete this building because it has associated rooms. Please delete all rooms in this building first." });
                }
                throw;
            }
        }
    }
}
