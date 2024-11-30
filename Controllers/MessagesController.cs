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
    }
}
