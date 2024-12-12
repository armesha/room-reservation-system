using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public IEnumerable<Event> GetAllEvents()
        {
            return _eventRepository.GetAllEvents();
        }

        public Event GetEventById(int eventId)
        {
            return _eventRepository.GetEventById(eventId);
        }

        public Event GetEventByBookingId(int bookingId)
        {
            return _eventRepository.GetEventByBookingId(bookingId);
        }

        public void AddEvent(Event eventEntity)
        {
            _eventRepository.AddEvent(eventEntity);
        }

        public void UpdateEvent(Event eventEntity)
        {
            _eventRepository.UpdateEvent(eventEntity);
        }

        public void DeleteEvent(int eventId)
        {
            _eventRepository.DeleteEvent(eventId);
        }

        public IEnumerable<Event> GetEventHierarchy(int? parentId = null)
        {
            return _eventRepository.GetEventHierarchy(parentId);
        }

        public IEnumerable<Event> GetUpcomingEvents()
        {
            return _eventRepository.GetUpcomingEvents();
        }

        public IEnumerable<EventBookingDetail> GetEventBookingDetails()
        {
            return _eventRepository.GetEventBookingDetails();
        }
    }
}
