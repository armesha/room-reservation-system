using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace RoomReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // GET: api/events
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IEnumerable<Event>> GetAllEvents([FromQuery] int? limit = 10, [FromQuery] int? offset = 0, [FromQuery] bool showAll = false)
        {
            var events = _eventService.GetAllEvents();
            var now = DateTime.UtcNow;

            // Event filtering
            if (!showAll)
            {
                events = events.Where(e => e.EventDate >= now);
            }
            else if (!User.IsInRole("Administrator"))
            {
                // For non-admins, show events not older than one month
                var oneMonthAgo = now.AddMonths(-1);
                events = events.Where(e => e.EventDate >= oneMonthAgo);
            }

            // Total number of events after filtering
            var totalCount = events.Count();

            // If user is not authorized, hide certain fields
            if (!User.Identity.IsAuthenticated)
            {
                events = events.Select(e => new Event
                {
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventDate = e.EventDate,
                    Description = e.Description
                });
            }

            // Apply pagination
            var pagedEvents = events
                .Skip(offset ?? 0)
                .Take(limit ?? 10)
                .ToList();

            return Ok(new { 
                list = pagedEvents,
                count = totalCount
            });
        }

        // GET: api/events/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<Event> GetEventById(int id)
        {
            var eventEntity = _eventService.GetEventById(id);
            if (eventEntity == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            // If user is not authorized, hide certain fields
            if (!User.Identity.IsAuthenticated)
            {
                eventEntity = new Event
                {
                    EventId = eventEntity.EventId,
                    EventName = eventEntity.EventName,
                    EventDate = eventEntity.EventDate,
                    Description = eventEntity.Description
                };
            }

            return Ok(new { eventEntity });
        }

        // GET: api/events/booking/{bookingId}
        [HttpGet("booking/{bookingId}")]
        [Authorize(Roles = "Administrator,Registered User")]
        public ActionResult<Event> GetEventByBookingId(int bookingId)
        {
            var eventEntity = _eventService.GetEventByBookingId(bookingId);
            if (eventEntity == null)
            {
                return NotFound(new { message = "No event found for this booking." });
            }
            return Ok(new { eventEntity });
        }

        // GET: api/events/my/incomplete
        [HttpGet("my/incomplete")]
        [Authorize(Roles = "Administrator,Registered User")]
        public ActionResult<IEnumerable<Event>> GetMyIncompleteEvents()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var events = _eventService.GetAllEvents()
                .Where(e => e.CreatedBy == userId && e.EventDate > DateTime.UtcNow)
                .ToList();

            return Ok(new { 
                list = events,
                count = events.Count
            });
        }

        // POST: api/events
        [HttpPost]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult CreateEvent([FromBody] Event eventEntity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            eventEntity.CreatedBy = userId;
            eventEntity.CreatedAt = DateTime.UtcNow;

            _eventService.AddEvent(eventEntity);
            return CreatedAtAction(nameof(GetEventById), new { id = eventEntity.EventId }, new { eventEntity });
        }

        // PUT: api/events/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Registered User")]
        public ActionResult<Event> UpdateEvent(int id, [FromBody] Event eventEntity)
        {
            if (id != eventEntity.EventId)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            var existingEvent = _eventService.GetEventById(id);
            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Administrator" && existingEvent.CreatedBy != userId)
            {
                return Forbid();
            }

            // Preserve original creation info
            eventEntity.CreatedBy = existingEvent.CreatedBy;
            eventEntity.CreatedAt = existingEvent.CreatedAt;

            _eventService.UpdateEvent(eventEntity);
            
            // Get and return updated event
            var updatedEvent = _eventService.GetEventById(id);
            return Ok(new { eventEntity = updatedEvent });
        }

        // PATCH: api/events/{id}/details
        [HttpPatch("{id}/details")]
        [Authorize(Roles = "Administrator,Registered User")]
        public ActionResult<Event> UpdateEventDetails(int id, [FromBody] EventDetailsUpdateDto details)
        {
            var existingEvent = _eventService.GetEventById(id);
            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            if (!User.IsInRole("Administrator") && existingEvent.CreatedBy != userId)
            {
                return Forbid();
            }

            // Update only name and description
            existingEvent.EventName = details.EventName ?? existingEvent.EventName;
            existingEvent.Description = details.Description ?? existingEvent.Description;

            _eventService.UpdateEvent(existingEvent);
            
            var updatedEvent = _eventService.GetEventById(id);
            return Ok(new { eventEntity = updatedEvent });
        }

        public class EventDetailsUpdateDto
        {
            public string? EventName { get; set; }
            public string? Description { get; set; }
        }

        // DELETE: api/events/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteEvent(int id)
        {
            try
            {
                var existingEvent = _eventService.GetEventById(id);
                if (existingEvent == null)
                {
                    return NotFound(new { message = "Event not found." });
                }

                _eventService.DeleteEvent(id);
                return Ok(new { success = true, message = "Event deleted successfully." });
            }
            catch (OracleException ex)
            {
                if (ex.Number == 2292) // ORA-02292: integrity constraint violation
                {
                    return BadRequest(new { message = "Cannot delete this event because it has associated bookings or other records. Please delete associated records first." });
                }
                throw;
            }
        }

        // GET: api/events/hierarchy
        [HttpGet("hierarchy")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<Event>> GetEventHierarchy([FromQuery] int? parentId = null)
        {
            return Ok(_eventService.GetEventHierarchy(parentId));
        }

        // GET: api/events/upcoming
        [HttpGet("upcoming")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<Event>> GetUpcomingEvents()
        {
            return Ok(_eventService.GetUpcomingEvents());
        }

        // GET: api/events/booking-details
        [HttpGet("booking-details")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<EventBookingDetail>> GetEventBookingDetails()
        {
            return Ok(_eventService.GetEventBookingDetails());
        }
    }
}
