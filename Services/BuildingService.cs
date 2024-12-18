using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _buildingRepository;

        public BuildingService(IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        public (IEnumerable<Building> Buildings, int TotalCount) GetAllBuildings(BuildingFilterParameters filterParams)
        {
            return _buildingRepository.GetAllBuildings(filterParams);
        }

        public Building GetBuildingById(int buildingId)
        {
            return _buildingRepository.GetBuildingById(buildingId);
        }

        public void AddBuilding(Building building)
        {
            _buildingRepository.AddBuilding(building);
        }

        public void UpdateBuilding(Building building)
        {
            _buildingRepository.UpdateBuilding(building);
        }

        public void DeleteBuilding(int buildingId)
        {
            _buildingRepository.DeleteBuilding(buildingId);
        }
    }
}
