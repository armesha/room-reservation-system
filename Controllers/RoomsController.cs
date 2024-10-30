//Controllers/RoomsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomReservationSystem.Data;
using RoomReservationSystem.DTOs.Rooms;
using RoomReservationSystem.Models;

namespace RoomReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/rooms
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoom([FromBody] RoomCreateRequest request)
        {
            var room = new Room
            {
                Name = request.Name,
                Capacity = request.Capacity
                // Initialize additional properties if necessary
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var response = new RoomResponse
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity
                // Map additional properties if necessary
            };

            return CreatedAtAction(nameof(GetRoomById), new { id = room.Id }, response);
        }

        // GET: api/rooms/{id}
        [HttpGet("{id}")]
        [Authorize] // Accessible by authenticated users
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var response = new RoomResponse
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity
                // Map additional properties if necessary
            };

            return Ok(response);
        }

        // Additional CRUD endpoints can be implemented here
    }
}
