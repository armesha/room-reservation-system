// Repositories/IMessageRepository.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface IMessageRepository
    {
        IEnumerable<Message> GetAllMessages();
        IEnumerable<Message> GetMessagesByUserId(int userId);
        void AddMessage(Message message);
        Message GetMessageById(int messageId);
        IEnumerable<Message> GetNotificationsForUser(int userId);
        void DeleteMessage(int messageId);
        void DeleteAllNotifications(int userId);
    }
}
