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
        private readonly ICountryRepository _countryRepository;

        public UsersController(
            IUserRepository userRepository, 
            IRoleRepository roleRepository, 
            IUserService userService,
            ICountryRepository countryRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userService = userService;
            _countryRepository = countryRepository;
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

        // PUT: /api/users/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = _userRepository.GetUserById(id);
            if (existingUser == null)
                return NotFound(new { message = "User not found." });

            // Update user fields if they are provided
            if (request.Username != null)
                existingUser.Username = request.Username;
            if (request.Email != null)
                existingUser.Email = request.Email;
            if (request.RoleName != null)
                existingUser.RoleName = request.RoleName;
            if (request.Name != null)
                existingUser.Name = request.Name;
            if (request.Surname != null)
                existingUser.Surname = request.Surname;
            if (request.Phone != null)
                existingUser.Phone = request.Phone;
            
            // Update country if provided
            if (request.Code != null)
            {
                if (!string.IsNullOrEmpty(request.Code))
                {
                    var country = _countryRepository.GetCountryByCode(request.Code);
                    if (country == null)
                        return BadRequest(new { message = "Invalid country code." });
                }
                _countryRepository.SetUserCountry(id, request.Code);
            }

            // Update user in database
            _userRepository.UpdateUser(existingUser);

            // Get updated user
            var updatedUser = _userRepository.GetUserById(id);
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
                Country = GetCountryName(_countryRepository.GetUserCountryCode(updatedUser.UserId))
            };

            return Ok(new { user = userResponse });
        }

        // PUT: /api/users/{id}/country
        [HttpPut("{id}/country")]
        public IActionResult UpdateUserCountry(int id, [FromBody] string countryCode)
        {
            var user = _userRepository.GetUserById(id);
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
