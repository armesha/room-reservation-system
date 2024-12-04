// Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Models.Auth;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.Services;
using System.Collections.Generic;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserService _userService;

        public UsersController(IUserRepository userRepository, IRoleRepository roleRepository, IUserService userService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userService = userService;
        }

        // GET: /api/users
        [HttpGet]
        public ActionResult<IEnumerable<UserResponse>> GetAllUsers([FromQuery] UserFilterParameters parameters)
        {
            var users = _userRepository.GetPaginatedUsers(parameters);
            var totalCount = _userRepository.GetTotalUsersCount();
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

            return Ok(new { 
                list = userResponses,
                totalCount = totalCount,
                offset = parameters.Offset,
                count = parameters.Count
            });
        }

        // GET: /api/users/{id}
        [HttpGet("{id}")]
        public ActionResult<UserResponse> GetUserById(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
                return BadRequest(new { message = "User role not found." });

            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = role.RoleName,
                RegistrationDate = user.RegistrationDate
            };

            return Ok(new { user = userResponse });
        }

        // POST: /api/users
        [HttpPost]
        public IActionResult AddUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _userRepository.AddUser(user);

            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
                return BadRequest(new { message = "User role not found." });

            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = role.RoleName,
                RegistrationDate = user.RegistrationDate
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new { user = userResponse });
        }

        // POST: /api/users/create
        [HttpPost("create")]
        [Authorize(Roles = "Administrator")]
        public IActionResult CreateUser([FromBody] AdminUserCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _userService.AdminCreateUser(request);
            if (response.Success)
            {
                var user = _userRepository.GetUserByUsername(request.Username);
                if (user == null)
                    return BadRequest(new { message = "User creation failed." });

                var role = _roleRepository.GetRoleById(user.RoleId);
                if (role == null)
                    return BadRequest(new { message = "User role not found." });

                var userResponse = new UserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = role.RoleName,
                    RegistrationDate = user.RegistrationDate
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new { user = userResponse });
            }

            return BadRequest(new { message = response.Message });
        }

        // PUT: /api/users/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = _userRepository.GetUserById(id);
            if (existingUser == null)
                return NotFound(new { message = "User not found." });

            existingUser.Username = request.Username;
            existingUser.Email = request.Email;
            existingUser.RoleName = request.RoleName;  // Set RoleName, which will be used to get RoleId

            _userRepository.UpdateUser(existingUser);

            var updatedUser = _userRepository.GetUserById(id);
            var updatedRole = _roleRepository.GetRoleById(updatedUser.RoleId);

            var userResponse = new UserResponse
            {
                UserId = updatedUser.UserId,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                Role = updatedRole.RoleName,
                RegistrationDate = updatedUser.RegistrationDate
            };

            return Ok(new { user = userResponse });
        }

        // DELETE: /api/users/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            try
            {
                _userRepository.DeleteUser(id);

                var role = _roleRepository.GetRoleById(user.RoleId);
                var userResponse = new UserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = role != null ? role.RoleName : "Unknown",
                    RegistrationDate = user.RegistrationDate
                };

                return Ok(new { message = "User deleted successfully.", user = userResponse });
            }
            catch (OracleException ex) when (ex.Number == 2292)
            {
                // ORA-02292: integrity constraint violated - child record found
                return BadRequest(new { message = "Cannot delete user because there are related logs." });
            }
            catch (Exception)
            {
                // Optionally log the exception here using a logging framework
                return StatusCode(500, new { message = "An error occurred while deleting the user." });
            }
        }
    }
}
