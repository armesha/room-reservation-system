using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface IEquipmentRepository
    {
        IEnumerable<Equipment> GetAllEquipment();
        Equipment GetEquipmentById(int id);
        Equipment CreateEquipment(Equipment equipment);
        Equipment UpdateEquipment(Equipment equipment);
        void DeleteEquipment(int id);
        IEnumerable<Equipment> GetEquipmentByRoomId(int roomId);
        void AddEquipmentToRoom(int roomId, int equipmentId);
        void RemoveEquipmentFromRoom(int roomId, int equipmentId);
        void UpdateRoomEquipment(int roomId, List<int> equipmentIds);
        bool IsEquipmentInRoom(int roomId, int equipmentId);
    }
}
