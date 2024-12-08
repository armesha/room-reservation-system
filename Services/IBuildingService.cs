using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public interface IBuildingService
    {
        (IEnumerable<Building> Buildings, int TotalCount) GetAllBuildings(BuildingFilterParameters filterParams);
        Building GetBuildingById(int buildingId);
        void AddBuilding(Building building);
        void UpdateBuilding(Building building);
        void DeleteBuilding(int buildingId);
    }
}
