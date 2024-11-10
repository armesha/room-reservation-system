using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using System.Security.Claims;
using System.Collections.Generic;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET: /api/bookings
        [HttpGet]
        [Authorize(Roles = "Administrator,Registered User")]
        public ActionResult<IEnumerable<Booking>> GetAllBookings()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Administrator")
            {
                var allBookings = _bookingService.GetAllBookingsForAdmin();
                return Ok(allBookings);
            }
            else
            {
                var userBookings = _bookingService.GetAllBookingsForUser(userId);
                return Ok(userBookings);
            }
        }

        // GET: /api/bookings/all
        [HttpGet("all")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<Booking>> GetAllBookingsForAdmin()
        {
            var allBookings = _bookingService.GetAllBookingsForAdmin();
            return Ok(allBookings);
        }

        // GET: /api/bookings/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator,Registered User")]
        public ActionResult<Booking> GetBookingById(int id)
        {
            var booking = _bookingService.GetBookingById(id);
            if (booking == null)
                return NotFound(new { message = "Booking not found." });

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Administrator" && booking.UserId != userId)
                return Forbid();

            return Ok(booking);
        }

        // POST: /api/bookings
        [HttpPost]
        [Authorize(Roles = "Registered User")]
        public IActionResult AddBooking([FromBody] Booking booking)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirstValue("UserId"));
            booking.UserId = userId;
            booking.Status = "Pending"; // Setting Status here

            _bookingService.AddBooking(booking);
            return CreatedAtAction(nameof(GetBookingById), new { id = booking.BookingId }, booking);
        }

        // PUT: /api/bookings/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult UpdateBooking(int id, [FromBody] Booking booking)
        {
            if (id != booking.BookingId)
                return BadRequest(new { message = "ID mismatch." });

            var existingBooking = _bookingService.GetBookingById(id);
            if (existingBooking == null)
                return NotFound(new { message = "Booking not found." });

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Administrator" && existingBooking.UserId != userId)
                return Forbid();

            booking.UserId = existingBooking.UserId;

            _bookingService.UpdateBooking(booking);
            return NoContent();
        }

        // DELETE: /api/bookings/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult DeleteBooking(int id)
        {
            var existingBooking = _bookingService.GetBookingById(id);
            if (existingBooking == null)
                return NotFound(new { message = "Booking not found." });

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role != "Administrator" && existingBooking.UserId != userId)
                return Forbid();

            _bookingService.DeleteBooking(id);
            return NoContent();
        }
    }
}
