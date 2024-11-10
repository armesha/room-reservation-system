// Services/IBuildingService.cs
using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public interface IBuildingService
    {
        IEnumerable<Building> GetAllBuildings();
        Building GetBuildingById(int buildingId);
        void AddBuilding(Building building);
        void UpdateBuilding(Building building);
        void DeleteBuilding(int buildingId);
    }
}
