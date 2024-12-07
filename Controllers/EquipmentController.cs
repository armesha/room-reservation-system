using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public EquipmentController(IEquipmentRepository equipmentRepository)
        {
            _equipmentRepository = equipmentRepository;
        }

        // GET: api/equipment
        [HttpGet]
        public ActionResult<IEnumerable<Equipment>> GetAllEquipment()
        {
            var equipment = _equipmentRepository.GetAllEquipment();
            return Ok(new { equipment = equipment });
        }
    }
}
