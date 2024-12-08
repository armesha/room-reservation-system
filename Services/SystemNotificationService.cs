using RoomReservationSystem.Models;
using System;

namespace RoomReservationSystem.Services
{
    public class SystemNotificationService
    {
        private readonly IMessageService _messageService;

        public SystemNotificationService(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public void NotifyReservationCreated(int userId, string roomName, DateTime startTime, DateTime endTime)
        {
            var message = new Message
            {
                SenderId = null,
                ReceiverId = userId,
                Subject = "New Reservation Created",
                Body = $"Your reservation for room '{roomName}' has been successfully created.\n" +
                      $"Start time: {startTime:g}\n" +
                      $"End time: {endTime:g}\n\n" +
                      "Please wait for the invoice to be generated and proceed with the payment.",
                SentAt = DateTime.UtcNow
            };
            _messageService.SendMessage(message);
        }

        public void NotifyInvoicePaid(int userId, string invoiceNumber, decimal amount)
        {
            var message = new Message
            {
                SenderId = null,
                ReceiverId = userId,
                Subject = "Payment Confirmed",
                Body = $"Payment for invoice #{invoiceNumber} has been confirmed.\n" +
                      $"Amount paid: ${amount:F2}\n\n" +
                      "Your reservation is now fully confirmed. Thank you for your payment!",
                SentAt = DateTime.UtcNow
            };
            _messageService.SendMessage(message);
        }

        public void NotifyNewLogin(int userId, string location, string deviceInfo)
        {
            var message = new Message
            {
                SenderId = null,
                ReceiverId = userId,
                Subject = "New Login Detected",
                Body = $"A new login to your account has been detected.\n" +
                      $"Time: {DateTime.UtcNow:g} UTC\n" +
                      $"Location: {location}\n" +
                      $"Device: {deviceInfo}\n\n" +
                      "If this wasn't you, please contact the administrator immediately.",
                SentAt = DateTime.UtcNow
            };
            _messageService.SendMessage(message);
        }

        public void NotifyReservationDeleted(int userId, string roomName, DateTime startTime, string reason)
        {
            var message = new Message
            {
                SenderId = null,
                ReceiverId = userId,
                Subject = "Reservation Cancelled by Administrator",
                Body = $"Your reservation for room '{roomName}' starting at {startTime:g} has been cancelled by an administrator.\n" +
                      $"Reason: {reason}\n\n" +
                      "If you have any questions, please contact the administrator.",
                SentAt = DateTime.UtcNow
            };
            _messageService.SendMessage(message);
        }

        public void SendWelcomeMessage(int userId, string username)
        {
            var message = new Message
            {
                SenderId = null,
                ReceiverId = userId,
                Subject = "Welcome to Room Reservation System",
                Body = $"Dear {username},\n\n" +
                      "Welcome to our Room Reservation System! We're excited to have you on board.\n\n" +
                      "Here's what you can do with our system:\n" +
                      "- Browse available rooms\n" +
                      "- Make reservations\n" +
                      "- Manage your bookings\n" +
                      "- Communicate with administrators\n\n" +
                      "If you have any questions, don't hesitate to contact our support team.\n\n" +
                      "Best regards,\n" +
                      "Room Reservation System Team",
                SentAt = DateTime.UtcNow
            };
            _messageService.SendMessage(message);
        }
    }
}
