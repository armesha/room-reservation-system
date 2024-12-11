using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public interface IMessageService
    {
        IEnumerable<Message> GetMessagesForUser(int userId);
        IEnumerable<Message> GetAllMessages(); 
        void SendMessage(Message message);
        Message GetMessageById(int messageId);
        IEnumerable<Message> GetNotificationsForUser(int userId);
        void DeleteMessage(int messageId);
        void DeleteAllNotifications(int userId);
    }
}
