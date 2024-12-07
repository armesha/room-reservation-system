using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public EquipmentController(IEquipmentRepository equipmentRepository)
        {
            _equipmentRepository = equipmentRepository;
        }

        // GET: api/equipment
        [HttpGet]
        public ActionResult<IEnumerable<Equipment>> GetAllEquipment()
        {
            var equipment = _equipmentRepository.GetAllEquipment();
            return Ok(new { equipment = equipment });
        }

        // GET: api/equipment/{id}
        [HttpGet("{id}")]
        public ActionResult<Equipment> GetEquipment(int id)
        {
            var equipment = _equipmentRepository.GetEquipmentById(id);
            if (equipment == null)
            {
                return NotFound();
            }
            return Ok(equipment);
        }

        // GET: api/equipment/room/{roomId}
        [HttpGet("room/{roomId}")]
        public ActionResult<IEnumerable<Equipment>> GetEquipmentByRoom(int roomId)
        {
            var equipment = _equipmentRepository.GetEquipmentByRoomId(roomId);
            return Ok(new { equipment = equipment });
        }

        // POST: api/equipment
        [HttpPost]
        public ActionResult<Equipment> CreateEquipment(Equipment equipment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdEquipment = _equipmentRepository.CreateEquipment(equipment);
            return CreatedAtAction(nameof(GetEquipment), new { id = createdEquipment.EquipmentId }, createdEquipment);
        }

        // PUT: api/equipment/{id}
        [HttpPut("{id}")]
        public ActionResult<Equipment> UpdateEquipment(int id, Equipment equipment)
        {
            try
            {
                equipment.EquipmentId = id;
                var updatedEquipment = _equipmentRepository.UpdateEquipment(equipment);
                return Ok(updatedEquipment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Failed to update equipment" });
            }
        }

        // DELETE: api/equipment/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteEquipment(int id)
        {
            try
            {
                _equipmentRepository.DeleteEquipment(id);
                return Ok(new { message = $"Equipment with ID {id} successfully deleted" });
            }
            catch
            {
                return NotFound(new { message = $"Equipment with ID {id} not found" });
            }
        }

        // POST: api/equipment/room/{roomId}/{equipmentId}
        [HttpPost("room/{roomId}/{equipmentId}")]
        public IActionResult AddEquipmentToRoom(int roomId, int equipmentId)
        {
            try
            {
                _equipmentRepository.AddEquipmentToRoom(roomId, equipmentId);
                return NoContent();
            }
            catch
            {
                return NotFound();
            }
        }

        // DELETE: api/equipment/room/{roomId}/{equipmentId}
        [HttpDelete("room/{roomId}/{equipmentId}")]
        public IActionResult RemoveEquipmentFromRoom(int roomId, int equipmentId)
        {
            try
            {
                _equipmentRepository.RemoveEquipmentFromRoom(roomId, equipmentId);
                return NoContent();
            }
            catch
            {
                return NotFound();
            }
        }

        // PUT: api/equipment/room/{roomId}
        [HttpPut("room/{roomId}")]
        public IActionResult UpdateRoomEquipment(int roomId, List<int> equipmentIds)
        {
            try
            {
                _equipmentRepository.UpdateRoomEquipment(roomId, equipmentIds);
                return NoContent();
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
