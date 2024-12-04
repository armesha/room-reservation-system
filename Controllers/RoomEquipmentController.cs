using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomEquipmentController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentRepository _equipmentRepository;

        public RoomEquipmentController(IRoomRepository roomRepository, IEquipmentRepository equipmentRepository)
        {
            _roomRepository = roomRepository;
            _equipmentRepository = equipmentRepository;
        }

        [HttpPost("{roomId}/equipment/{equipmentId}")]
        public IActionResult AddEquipmentToRoom(int roomId, int equipmentId)
        {
            var room = _roomRepository.GetRoomById(roomId);
            if (room == null)
                return NotFound($"Room with ID {roomId} not found");

            var equipment = _equipmentRepository.GetEquipmentById(equipmentId);
            if (equipment == null)
                return NotFound($"Equipment with ID {equipmentId} not found");

            if (_equipmentRepository.IsEquipmentInRoom(roomId, equipmentId))
                return BadRequest($"Equipment with ID {equipmentId} is already in room {roomId}");

            try
            {
                _equipmentRepository.AddEquipmentToRoom(roomId, equipmentId);
                var updatedEquipmentList = _equipmentRepository.GetEquipmentByRoomId(roomId);
                return Ok(updatedEquipmentList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while adding equipment: {ex.Message}");
            }
        }

        [HttpDelete("{roomId}/equipment/{equipmentId}")]
        public IActionResult RemoveEquipmentFromRoom(int roomId, int equipmentId)
        {
            var room = _roomRepository.GetRoomById(roomId);
            if (room == null)
                return NotFound($"Room with ID {roomId} not found");

            var equipment = _equipmentRepository.GetEquipmentById(equipmentId);
            if (equipment == null)
                return NotFound($"Equipment with ID {equipmentId} not found");

            if (!_equipmentRepository.IsEquipmentInRoom(roomId, equipmentId))
                return NotFound($"Equipment with ID {equipmentId} is not found in room {roomId}");

            try
            {
                Console.WriteLine($"Removing equipment {equipmentId} from room {roomId}");
                _equipmentRepository.RemoveEquipmentFromRoom(roomId, equipmentId);
                
                var updatedEquipmentList = _equipmentRepository.GetEquipmentByRoomId(roomId).ToList();
                Console.WriteLine($"Updated equipment list count: {updatedEquipmentList.Count}");
                foreach (var eq in updatedEquipmentList)
                {
                    Console.WriteLine($"Equipment in list: ID={eq.EquipmentId}, Name={eq.Name}");
                }
                
                return Ok(updatedEquipmentList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return StatusCode(500, $"An error occurred while removing equipment from room: {ex.Message}");
            }
        }
    }
}
