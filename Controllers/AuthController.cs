using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models.Auth;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.Services;
using RoomReservationSystem.Models;
using System.Security.Claims;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public AuthController(IUserService userService, IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userService = userService;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _userService.Register(request);
            if (response.Success)
            {
                var user = _userRepository.GetUserByUsername(request.Username);
                if (user == null)
                    return BadRequest(new { message = "User registration failed." });

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

            return BadRequest(new { message = response.Message });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _userService.Authenticate(request);
            if (response == null)
                return Unauthorized(new { message = "Invalid credentials." });

            return Ok(new
            {
                token = response.Token,
                username = response.Username,
                role = response.Role,
                userId = response.UserId
            });
        }

        // POST: /api/auth/logout
        [HttpPost("logout")]
        [Authorize] // Ensure that only authenticated users can access this endpoint
        public IActionResult Logout()
        {
            // Extract the UserId from the JWT claims
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            // Fetch user details from the repository
            var user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Fetch the user's role name
            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
            {
                return BadRequest(new { message = "User role not found." });
            }

            // Prepare the UserResponse object
            var userResponse = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = role.RoleName,
                RegistrationDate = user.RegistrationDate
            };

            // Return the logout success message along with user information
            return Ok(new
            {
                message = "Logout successful.",
                user = userResponse
            });
        }
    }
}
