// Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.Services;
using System.Collections.Generic;
using System.Linq;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UsersController(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        // GET: /api/users
        [HttpGet]
        public ActionResult<IEnumerable<UserResponse>> GetAllUsers()
        {
            var users = _userRepository.GetAllUsers(); 
            var roles = _roleRepository.GetAllRoles(); 

            var userResponses = from user in users
                                join role in roles on user.RoleId equals role.RoleId
                                select new UserResponse
                                {
                                    UserId = user.UserId,
                                    Username = user.Username,
                                    Email = user.Email,
                                    Role = role.RoleName,
                                    RegistrationDate = user.RegistrationDate
                                };

            return Ok(userResponses);
        }

        // PUT: /api/users/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userRepository.GetUserById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            user.Username = request.Username;
            user.Email = request.Email;

            if (request.RoleName != null)
            {
                var role = _roleRepository.GetRoleByName(request.RoleName);
                if (role == null)
                    return BadRequest(new { message = "Invalid role name." });

                user.RoleId = role.RoleId;
            }

            _userRepository.UpdateUser(user); 
            return NoContent();
        }

        // DELETE: /api/users/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            _userRepository.DeleteUser(id);
            return NoContent();
        }
    }
}
