using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;
using System.Security.Claims;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LogsController : ControllerBase
    {
        private readonly ILogRepository _logRepository;

        public LogsController(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        // GET: api/logs
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<Log>> GetAllLogs()
        {
            var logs = _logRepository.GetAllLogs();
            return Ok(new { list = logs });
        }

        // GET: api/logs/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<Log> GetLogById(int id)
        {
            var log = _logRepository.GetLogById(id);
            if (log == null)
                return NotFound(new { message = "Log not found." });

            return Ok(new { log });
        }

        // GET: api/logs/user
        [HttpGet("user")]
        public ActionResult<IEnumerable<Log>> GetUserLogs()
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "Username not found." });
            }

            var logs = _logRepository.GetLogsByUsername(username);
            return Ok(new { list = logs });
        }
    }
}
