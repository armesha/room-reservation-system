using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentRepository _equipmentRepository;

        public RoomsController(IRoomRepository roomRepository, IEquipmentRepository equipmentRepository)
        {
            _roomRepository = roomRepository;
            _equipmentRepository = equipmentRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<Room>> GetAllRooms(
            [FromQuery] int? limit = null, 
            [FromQuery] int? offset = null,
            [FromQuery] string? name = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? minCapacity = null,
            [FromQuery] int? maxCapacity = null,
            [FromQuery] List<int>? equipmentIds = null,
            [FromQuery] int? buildingId = null)
        {
            // For requests without a token, set a limit
            if (!User.Identity.IsAuthenticated)
            {
                const int maxLimit = 10; // Maximum number of rooms for public access
                if (!limit.HasValue || limit.Value > maxLimit)
                {
                    limit = maxLimit;
                }
            }

            var filters = new RoomFilterParameters
            {
                Name = name,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                MinCapacity = minCapacity,
                MaxCapacity = maxCapacity,
                EquipmentIds = equipmentIds,
                BuildingId = buildingId
            };

            var rooms = _roomRepository.GetAllRooms(limit, offset, filters);
            return Ok(new { list = rooms });
        }

        [HttpGet("random")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<Room>> GetRandomRooms([FromQuery] int count = 3)
        {
            if (count <= 0 || count > 10)
            {
                return BadRequest(new { message = "Count must be between 1 and 10" });
            }

            var rooms = _roomRepository.GetRandomRooms(count);
            return Ok(new { list = rooms });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<Room> GetRoomById(int id)
        {
            var room = _roomRepository.GetRoomById(id);
            if (room == null)
                return NotFound(new { message = "Room not found." });

            return Ok(new { room });
        }

        [HttpGet("{roomId}/reservations")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<object>> GetRoomReservations(int roomId)
        {
            // Get all reservations for the room
            var reservations = _roomRepository.GetRoomReservations(roomId, DateTime.MinValue, DateTime.MaxValue);
            
            // Transform reservations into time slots array
            var timeSlots = reservations
                .OrderBy(r => r.StartTime)
                .Select(r => new[] 
                {
                    r.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    r.EndTime.ToString("yyyy-MM-dd HH:mm")
                })
                .ToList();

            return Ok(timeSlots);
        }

        [HttpGet("{roomId}/utilization")]
        [AllowAnonymous]
        public ActionResult<decimal> GetRoomUtilization(
            int roomId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var room = _roomRepository.GetRoomById(roomId);
            if (room == null)
                return NotFound(new { message = "Room not found." });

            if (startDate >= endDate)
                return BadRequest(new { message = "Start date must be before end date." });

            var utilization = _roomRepository.GetRoomUtilization(roomId, startDate, endDate);
            return Ok(new { utilization });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult AddRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _roomRepository.AddRoom(room);
            return CreatedAtAction(nameof(GetRoomById), new { id = room.RoomId }, new { room });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult UpdateRoom(int id, [FromBody] RoomUpdateRequest updateRequest)
        {
            try 
            {
                var existingRoom = _roomRepository.GetRoomById(id);
                if (existingRoom == null)
                    return NotFound(new { message = "Room not found." });

                // Create Room object for update
                var room = new Room
                {
                    RoomId = id,
                    BuildingId = updateRequest.BuildingId,
                    RoomNumber = updateRequest.RoomNumber,
                    Capacity = updateRequest.Capacity,
                    Description = updateRequest.Description,
                    Price = updateRequest.Price,
                    IdFile = updateRequest.IdFile,
                    Equipment = new List<Equipment>()
                };

                // Add equipment
                if (updateRequest.Equipment != null && updateRequest.Equipment.Any())
                {
                    foreach (var equipRef in updateRequest.Equipment)
                    {
                        var existingEquipment = _equipmentRepository.GetEquipmentById(equipRef.EquipmentId);
                        if (existingEquipment != null)
                        {
                            room.Equipment.Add(existingEquipment);
                        }
                    }
                }
                
                _roomRepository.UpdateRoom(room);
                
                // Get and return updated room
                var updatedRoom = _roomRepository.GetRoomById(id);
                return Ok(new { room = updatedRoom });
            }
            catch (OracleException ex) when (ex.Number == 1) // ORA-00001
            {
                return BadRequest(new { message = $"Room number '{updateRequest.RoomNumber}' already exists in building {updateRequest.BuildingId}." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteRoom(int id)
        {
            try
            {
                var existingRoom = _roomRepository.GetRoomById(id);
                if (existingRoom == null)
                    return NotFound(new { message = "Room not found." });

                _roomRepository.DeleteRoom(id);
                return Ok(new { success = true, message = "Room deleted successfully." });
            }
            catch (OracleException ex)
            {
                if (ex.Number == 2292) // ORA-02292: integrity constraint violation
                {
                    return BadRequest(new { message = "Cannot delete this room because it has active bookings or events. Please delete associated records first." });
                }
                throw;
            }
        }
    }
}
