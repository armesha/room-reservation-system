using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;

        public RoomsController(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        // GET: /api/rooms
        [HttpGet]
        [Authorize(Roles = "Administrator,Registered User,Unauthenticated User")]
        public ActionResult<IEnumerable<Room>> GetAllRooms()
        {
            var rooms = _roomRepository.GetAllRooms();
            return Ok(rooms);
        }

        // GET: /api/rooms/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,Registered User,Unauthenticated User")]
        public ActionResult<Room> GetRoomById(int id)
        {
            var room = _roomRepository.GetRoomById(id);
            if (room == null)
                return NotFound(new { message = "Room not found." });

            return Ok(room);
        }

        // POST: /api/rooms
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult AddRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _roomRepository.AddRoom(room);
            return CreatedAtAction(nameof(GetRoomById), new { id = room.RoomId }, room);
        }

        // PUT: /api/rooms/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult UpdateRoom(int id, [FromBody] Room room)
        {
            if (id != room.RoomId)
                return BadRequest(new { message = "ID mismatch." });

            var existingRoom = _roomRepository.GetRoomById(id);
            if (existingRoom == null)
                return NotFound(new { message = "Room not found." });

            _roomRepository.UpdateRoom(room);
            return NoContent();
        }

        // DELETE: /api/rooms/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteRoom(int id)
        {
            var existingRoom = _roomRepository.GetRoomById(id);
            if (existingRoom == null)
                return NotFound(new { message = "Room not found." });

            _roomRepository.DeleteRoom(id);
            return NoContent();
        }
    }
}
