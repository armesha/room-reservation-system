// Repositories/IEventRepository.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface IEventRepository
    {
        IEnumerable<Event> GetAllEvents();
        Event GetEventById(int eventId);
        Event GetEventByBookingId(int bookingId);
        void AddEvent(Event eventEntity);
        void UpdateEvent(Event eventEntity);
        void DeleteEvent(int eventId);
    }
}
