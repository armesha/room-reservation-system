using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using System.Linq;

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

            // Фильтрация событий
            if (!showAll)
            {
                events = events.Where(e => e.EventDate >= now);
            }
            else if (!User.IsInRole("Administrator"))
            {
                // Для не-админов показываем события не старше месяца
                var oneMonthAgo = now.AddMonths(-1);
                events = events.Where(e => e.EventDate >= oneMonthAgo);
            }

            // Общее количество событий после фильтрации
            var totalCount = events.Count();

            // Если пользователь не авторизован, скрываем некоторые поля
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

            // Применяем пагинацию
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

            // Если пользователь не авторизован, скрываем некоторые поля
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
            
            // Получаем и возвращаем обновленное событие
            var updatedEvent = _eventService.GetEventById(id);
            return Ok(new { eventEntity = updatedEvent });
        }

        // DELETE: api/events/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,Registered User")]
        public IActionResult DeleteEvent(int id)
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

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Administrator" && existingEvent.CreatedBy != userId)
            {
                return Forbid();
            }

            _eventService.DeleteEvent(id);
            return NoContent();
        }
    }
}
