// Services/MessageService.cs
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;

        public MessageService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public IEnumerable<Message> GetMessagesForUser(int userId)
        {
            return _messageRepository.GetMessagesByUserId(userId);
        }

        public IEnumerable<Message> GetAllMessages()
        {
            return _messageRepository.GetAllMessages();
        }

        public void SendMessage(Message message)
        {
            message.SentAt = DateTime.UtcNow;
            _messageRepository.AddMessage(message);
        }
    }
}
