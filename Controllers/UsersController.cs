using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Models.Auth;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.Services;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleRepository _roleRepository;
        private readonly ICountryRepository _countryRepository;

        public UsersController(
            IUserService userService,
            IRoleRepository roleRepository,
            ICountryRepository countryRepository)
        {
            _userService = userService;
            _roleRepository = roleRepository;
            _countryRepository = countryRepository;
        }

        // GET: /api/users
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult<IEnumerable<UserResponse>> GetAllUsers([FromQuery] UserFilterParameters parameters)
        {
            var users = _userService.GetPaginatedUsers(parameters);
            var totalCount = _userService.GetTotalUsersCount();
            var roles = _roleRepository.GetAllRoles();

            var userResponses = from user in users
                                join role in roles on user.RoleId equals role.RoleId
                                select new UserResponse
                                {
                                    UserId = user.UserId,
                                    Username = user.Username,
                                    Email = user.Email,
                                    Role = role.RoleName,
                                    RegistrationDate = user.RegistrationDate,
                                    Name = user.Name,
                                    Surname = user.Surname,
                                    Phone = user.Phone,
                                    Country = GetCountryName(_countryRepository.GetUserCountryCode(user.UserId))
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
        [Authorize(Roles = "Administrator")]
        public ActionResult<UserResponse> GetUserById(int id)
        {
            var user = _userService.GetUserById(id);
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
                RegistrationDate = user.RegistrationDate,
                Name = user.Name,
                Surname = user.Surname,
                Phone = user.Phone,
                Country = GetCountryName(_countryRepository.GetUserCountryCode(user.UserId))
            };

            return Ok(new { user = userResponse });
        }

        // POST: /api/users
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult AddUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _userService.AddUser(user);

            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
                return BadRequest(new { message = "User role not found." });

            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = role.RoleName,
                RegistrationDate = user.RegistrationDate,
                Name = user.Name,
                Surname = user.Surname,
                Phone = user.Phone,
                Country = GetCountryName(_countryRepository.GetUserCountryCode(user.UserId))
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
                var user = _userService.GetUserByUsername(request.Username);
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
                    RegistrationDate = user.RegistrationDate,
                    Name = user.Name,
                    Surname = user.Surname,
                    Phone = user.Phone,
                    Country = GetCountryName(_countryRepository.GetUserCountryCode(user.UserId))
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new { user = userResponse });
            }

            return BadRequest(new { message = response.Message });
        }

        // GET: /api/users/me
        [HttpGet("me")]
        [Authorize]
        public ActionResult<User> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var user = _userService.GetUserById(userId);
            return user == null ? NotFound() : Ok(user);
        }

        // PUT: /api/users/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<UserResponse> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var existingUser = _userService.GetUserById(id);
            if (existingUser == null)
                return NotFound(new { message = "User not found." });

            var updatedUser = _userService.UpdateUser(id, request);
            if (updatedUser == null)
                return StatusCode(500, new { message = "Failed to update user." });

            var role = _roleRepository.GetRoleById(updatedUser.RoleId);
            var userResponse = new UserResponse
            {
                UserId = updatedUser.UserId,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                Role = role != null ? role.RoleName : "Unknown",
                RegistrationDate = updatedUser.RegistrationDate,
                Name = updatedUser.Name,
                Surname = updatedUser.Surname,
                Phone = updatedUser.Phone,
                Country = GetCountryName(updatedUser.Code)
            };

            return Ok(new { user = userResponse });
        }

        // PUT: /api/users/me/password
        [HttpPut("me/password")]
        [Authorize]
        public ActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var success = _userService.ChangePassword(userId, request.CurrentPassword, request.NewPassword);
            return success ? Ok(new { message = "Password changed successfully." }) 
                          : BadRequest(new { message = "Failed to change password. Please check your current password." });
        }

        // PUT: /api/users/{id}/country
        [HttpPut("{id}/country")]
        [Authorize(Roles = "Administrator")]
        public IActionResult UpdateUserCountry(int id, [FromBody] string countryCode)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (!string.IsNullOrEmpty(countryCode))
            {
                var country = _countryRepository.GetCountryByCode(countryCode);
                if (country == null)
                    return BadRequest(new { message = "Invalid country code." });
            }

            _countryRepository.SetUserCountry(id, countryCode);

            return Ok(new { message = "User country updated successfully." });
        }

        // DELETE: /api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteUser(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            try
            {
                _userService.DeleteUser(id);

                var role = _roleRepository.GetRoleById(user.RoleId);
                var userResponse = new UserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = role != null ? role.RoleName : "Unknown",
                    RegistrationDate = user.RegistrationDate,
                    Name = user.Name,
                    Surname = user.Surname,
                    Phone = user.Phone,
                    Country = GetCountryName(_countryRepository.GetUserCountryCode(user.UserId))
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

        private string GetCountryName(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return null;

            var country = _countryRepository.GetCountryByCode(countryCode);
            return country?.CountryName;
        }
    }
}
