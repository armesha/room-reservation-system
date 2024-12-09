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
        [Authorize]
        public ActionResult<IEnumerable<object>> GetAllUsers([FromQuery] UserFilterParameters parameters)
        {
            var users = _userService.GetPaginatedUsers(parameters);
            var totalCount = _userService.GetTotalUsersCount();

            // Get the current user's ID and role
            var currentUserId = int.Parse(User.FindFirstValue("UserId"));
            var isAdmin = User.IsInRole("Administrator");

            var userResponses = users.Select(user =>
            {
                var role = _roleRepository.GetRoleById(user.RoleId);
                if (role == null) return null;

                if (isAdmin || user.UserId == currentUserId)
                {
                    return AdminUserResponse.FromUser(user, role.RoleName);
                }
                else
                {
                    return new BasicUserResponse
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Role = role.RoleName
                    };
                }
            }).Where(u => u != null);

            return Ok(new
            {
                list = userResponses,
                totalCount = totalCount,
                offset = parameters.Offset,
                count = parameters.Count
            });
        }

        // GET: /api/users/{id}
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetUserById(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
                return BadRequest(new { message = "User role not found." });

            // Get the current user's ID and role
            var currentUserId = int.Parse(User.FindFirstValue("UserId"));
            var isAdmin = User.IsInRole("Administrator");
            var isOwnProfile = currentUserId == id;

            object response;
            if (isAdmin || isOwnProfile)
            {
                response = AdminUserResponse.FromUser(user, role.RoleName);
            }
            else
            {
                response = new BasicUserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = role.RoleName
                };
            }

            return Ok(new { user = response });
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

            var response = AdminUserResponse.FromUser(user, role.RoleName);
            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new { user = response });
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

                var userResponse = AdminUserResponse.FromUser(user, role.RoleName);
                return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new { user = userResponse });
            }

            return BadRequest(new { message = response.Message });
        }

        // GET: /api/users/me
        [HttpGet("me")]
        [Authorize]
        public ActionResult<object> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var user = _userService.GetUserById(userId);
            if (user == null)
                return NotFound();

            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
                return BadRequest(new { message = "User role not found." });

            // Always return full data for own profile
            var response = AdminUserResponse.FromUser(user, role.RoleName);
            return Ok(new { user = response });
        }

        // PUT: /api/users/{id}
        [HttpPut("{id}")]
        [Authorize]
        public ActionResult<object> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var existingUser = _userService.GetUserById(id);
            if (existingUser == null)
                return NotFound(new { message = "User not found." });

            // Check if user is admin or updating their own profile
            var currentUserId = int.Parse(User.FindFirstValue("UserId"));
            var isAdmin = User.IsInRole("Administrator");
            
            if (!isAdmin && currentUserId != id)
            {
                return Forbid();
            }

            var updatedUser = _userService.UpdateUser(id, request);
            if (updatedUser == null)
                return StatusCode(500, new { message = "Failed to update user." });

            var role = _roleRepository.GetRoleById(updatedUser.RoleId);
            var userResponse = AdminUserResponse.FromUser(updatedUser, role.RoleName);
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
                var userResponse = AdminUserResponse.FromUser(user, role.RoleName);
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
