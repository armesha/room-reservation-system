// Repositories/IRoomRepository.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;
using System;

namespace RoomReservationSystem.Repositories
{
    public interface IRoomRepository
    {
        IEnumerable<Room> GetAllRooms(int? limit = null, int? offset = null, RoomFilterParameters filters = null);
        IEnumerable<Room> GetRandomRooms(int count);
        Room GetRoomById(int roomId);
        void AddRoom(Room room);
        void UpdateRoom(Room room);
        void DeleteRoom(int roomId);
        IEnumerable<DateTime> GetReservedDates(int roomId, DateTime startDate, DateTime endDate);
        IEnumerable<Booking> GetRoomReservations(int roomId, DateTime startDate, DateTime endDate);
        List<int> GetEquipmentByRoomId(int roomId);
        void AddEquipmentToRoom(int roomId, int equipmentId);
        void RemoveEquipmentFromRoom(int roomId, int equipmentId);
    }
}
