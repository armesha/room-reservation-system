using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using RoomReservationSystem.Repositories;
using System.Security.Claims;
using System.Collections.Generic;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Registered User")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IUserRepository _userRepository;

        public MessagesController(IMessageService messageService, IUserRepository userRepository)
        {
            _messageService = messageService;
            _userRepository = userRepository;
        }

        // GET: /api/messages
        [HttpGet]
        public ActionResult<IEnumerable<Message>> GetMessages()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var role = User.FindFirstValue(ClaimTypes.Role);

            if (role == "Administrator")
            {
                var allMessages = _messageService.GetAllMessages();
                return Ok(new { list = allMessages });
            }
            else
            {
                var messages = _messageService.GetMessagesForUser(userId);
                return Ok(new { list = messages });
            }
        }

        // POST: /api/messages
        [HttpPost]
        public IActionResult SendMessage([FromBody] MessageCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var senderIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(senderIdClaim, out int senderId))
            {
                return Unauthorized(new { message = "Invalid sender ID." });
            }

            var receiver = _userRepository.GetUserByUsername(request.ReceiverUsername);
            if (receiver == null)
                return BadRequest(new { message = "Receiver does not exist." });

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiver.UserId,
                Subject = request.Subject,
                Body = request.Body,
                SentAt = DateTime.UtcNow
            };

            _messageService.SendMessage(message);
            return Ok(new { message });
        }

        // GET: /api/messages/notifications
        [HttpGet("notifications")]
        public ActionResult<IEnumerable<Message>> GetNotifications()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var notifications = _messageService.GetNotificationsForUser(userId);
            return Ok(new { list = notifications });
        }

        // DELETE: /api/messages/notifications/{id}
        [HttpDelete("notifications/{id}")]
        public IActionResult DeleteNotification(int id)
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var notification = _messageService.GetMessageById(id);
            if (notification == null)
                return NotFound(new { message = "Notification not found." });

            if (notification.ReceiverId != userId)
                return Forbid();

            if (notification.SenderId != null)
                return BadRequest(new { message = "This is not a notification." });

            _messageService.DeleteMessage(id);
            return Ok();
        }

        // DELETE: /api/messages/notifications
        [HttpDelete("notifications")]
        public IActionResult DeleteAllNotifications()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            _messageService.DeleteAllNotifications(userId);
            return Ok();
        }

        // Utility method for creating system notifications
        private Message CreateNotification(int receiverId, string subject, string body)
        {
            var notification = new Message
            {
                SenderId = null,
                ReceiverId = receiverId,
                Subject = subject,
                Body = body,
                SentAt = DateTime.UtcNow
            };

            _messageService.SendMessage(notification);
            return notification;
        }
    }
}
