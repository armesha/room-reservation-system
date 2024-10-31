using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomReservationSystem.Data;
using RoomReservationSystem.DTOs.Bookings;
using RoomReservationSystem.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoomReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingCreateRequest request)
        {
            // Validate date range
            if (request.EndDate <= request.StartDate)
            {
                return BadRequest(new { message = "End date must be after start date." });
            }

            // Check room availability
            var roomUnavailable = await _context.Bookings
                .AnyAsync(b => b.RoomId == request.RoomId &&
                    ((request.StartDate >= b.StartDate && request.StartDate < b.EndDate) ||
                     (request.EndDate > b.StartDate && request.EndDate <= b.EndDate)));

            if (roomUnavailable)
            {
                return BadRequest(new { message = "Room is not available for the selected dates." });
            }

            // Get user ID from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdClaim.Value);

            var booking = new Booking
            {
                RoomId = request.RoomId,
                UserId = userId,
                StartDate = request.StartDate,
                EndDate = request.EndDate
                // Set additional properties as needed
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var response = new BookingResponse
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                UserId = booking.UserId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate
                // Map additional properties as needed
            };

            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, response);
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<IActionResult> GetUserBookings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdClaim.Value);

            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    RoomId = b.RoomId,
                    UserId = b.UserId,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate
                    // Map additional properties as needed
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/bookings/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Ensure the user has access to this booking
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdClaim.Value);

            if (booking.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var response = new BookingResponse
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                UserId = booking.UserId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate
                // Map additional properties as needed
            };

            return Ok(response);
        }

        // PUT: api/bookings/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingCreateRequest request)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Ensure the user has access to update this booking
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdClaim.Value);

            if (booking.UserId != userId)
            {
                return Forbid();
            }

            // Validate date range
            if (request.EndDate <= request.StartDate)
            {
                return BadRequest(new { message = "End date must be after start date." });
            }

            // Check room availability excluding the current booking
            var roomUnavailable = await _context.Bookings
                .AnyAsync(b => b.RoomId == request.RoomId && b.Id != id &&
                    ((request.StartDate >= b.StartDate && request.StartDate < b.EndDate) ||
                     (request.EndDate > b.StartDate && request.EndDate <= b.EndDate)));

            if (roomUnavailable)
            {
                return BadRequest(new { message = "Room is not available for the selected dates." });
            }

            // Update booking details
            booking.StartDate = request.StartDate;
            booking.EndDate = request.EndDate;
            // Update additional properties as needed

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/bookings/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Ensure the user has access to delete this booking
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdClaim.Value);

            if (booking.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/bookings/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .Select(b => new BookingResponse
                {
                    Id = b.Id,
                    RoomId = b.RoomId,
                    UserId = b.UserId,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate
                    // Map additional properties as needed
                })
                .ToListAsync();

            return Ok(bookings);
        }
    }
}