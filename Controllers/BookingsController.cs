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
        private readonly IEventService _eventService;

        public BookingsController(IBookingService bookingService, IEventService eventService)
        {
            _bookingService = bookingService;
            _eventService = eventService;
        }

        // GET: /api/bookings
        [HttpGet]
        [Authorize(Roles = "Administrator,Registered User")]
        public ActionResult<IEnumerable<Booking>> GetAllBookings(
            [FromQuery] int? limit = null,
            [FromQuery] int? offset = null,
            [FromQuery] int? roomId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string status = null,
            [FromQuery] bool? hasEvent = null,
            [FromQuery] bool onlyMine = false)
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            var filters = new BookingFilterParameters
            {
                RoomId = roomId,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                HasEvent = hasEvent,
                UserId = onlyMine ? userId : (role != "Administrator" ? userId : null)
            };

            // For non-authenticated users or non-admin users, set a maximum limit
            if (role != "Administrator")
            {
                const int maxLimit = 10;
                if (!limit.HasValue || limit.Value > maxLimit)
                {
                    limit = maxLimit;
                }
            }

            if (role == "Administrator" && !onlyMine)
            {
                var result = _bookingService.GetAllBookingsForAdmin(limit, offset, filters);
                return Ok(new { list = result.Bookings, total = result.TotalCount });
            }
            else
            {
                var result = _bookingService.GetAllBookingsForUser(userId, limit, offset, filters);
                return Ok(new { list = result.Bookings, total = result.TotalCount });
            }
        }

        // GET: /api/bookings/all
        [HttpGet("all")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<Booking>> GetAllBookingsForAdmin(
            [FromQuery] int? limit = null,
            [FromQuery] int? offset = null,
            [FromQuery] int? userId = null,
            [FromQuery] int? roomId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string status = null,
            [FromQuery] bool? hasEvent = null)
        {
            var filters = new BookingFilterParameters
            {
                UserId = userId,
                RoomId = roomId,
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                HasEvent = hasEvent
            };

            var allBookings = _bookingService.GetAllBookingsForAdmin(limit, offset, filters);
            return Ok(new { list = allBookings });
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

            return Ok(new { booking });
        }

        // POST: /api/bookings
        [HttpPost]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult AddBooking([FromBody] Booking booking)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            booking.UserId = userId;
            booking.Status = "Pending"; // Always set status to Pending

            _bookingService.AddBooking(booking);

            // Get the associated event if it was created
            Event? eventEntity = null;
            try 
            {
                eventEntity = _eventService.GetEventByBookingId(booking.BookingId);
            }
            catch
            {
                // If there's an error getting the event, we'll return null for the event
            }

            return CreatedAtAction(
                nameof(GetBookingById),
                new { id = booking.BookingId },
                new { booking, @event = eventEntity }
            );
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

            // Ensure the UserId remains unchanged
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

        // GET: /api/bookings/admin/invoices/unpaid
        [HttpGet("admin/invoices/unpaid")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<Invoice>> GetUnpaidInvoices()
        {
            var unpaidInvoices = _bookingService.GetUnpaidInvoices(); 
            return Ok(new { list = unpaidInvoices });
        }

        // GET: /api/bookings/admin/invoices/paid
        [HttpGet("admin/invoices/paid")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<Invoice>> GetPaidInvoices()
        {
            var paidInvoices = _bookingService.GetPaidInvoices(); 
            return Ok(new { list = paidInvoices });
        }

        // GET: /api/bookings/user/invoices
        [HttpGet("user/invoices")]
        [Authorize(Roles = "Administrator,Registered User")]
        public ActionResult<IEnumerable<Invoice>> GetUserInvoices()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var userInvoices = _bookingService.GetUserInvoices(userId); 
            return Ok(new { list = userInvoices });
        }

        // POST: /api/bookings/{id}/cancel
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult CancelBooking(int id)
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

            booking.Status = "Cancelled";
            _bookingService.UpdateBooking(booking);

            return Ok(new { success = true, message = "Booking cancelled successfully." });
        }

        // POST: /api/bookings/{id}/pay
        [HttpPost("{id}/pay")]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult PayInvoice(int id)
        {
            var success = _bookingService.MarkInvoiceAsPaid(id);
            if (!success)
                return NotFound(new { message = "Invoice not found." });

            return Ok(new { success = true, message = "Invoice marked as paid successfully." });
        }
    }
}
