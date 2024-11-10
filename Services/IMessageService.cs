// Services/IMessageService.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public interface IMessageService
    {
        IEnumerable<Message> GetMessagesForUser(int userId);
        IEnumerable<Message> GetAllMessages(); // Added for Admin
        void SendMessage(Message message);
    }
}
