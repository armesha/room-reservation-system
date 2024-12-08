using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System;
using System.Threading.Tasks;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Registered User")]
    public class RoomAnalyticsController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;

        public RoomAnalyticsController(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        [HttpGet("optimal")]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult FindOptimalRooms(
            [FromQuery] int capacity,
            [FromQuery] decimal maxPrice,
            [FromQuery] string[] equipment,
            [FromQuery] DateTime date)
        {
            try
            {
                var rooms = _roomRepository.FindOptimalRooms(capacity, maxPrice, equipment, date);
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("occupancy/{roomId}")]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult AnalyzeRoomOccupancy(
            int roomId,
            [FromQuery] DateTime startDate,
            [FromQuery] int daysAhead)
        {
            try
            {
                var occupancy = _roomRepository.AnalyzeRoomOccupancy(roomId, startDate, daysAhead);
                return Ok(occupancy);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
