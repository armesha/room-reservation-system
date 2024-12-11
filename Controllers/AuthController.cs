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

        private const string JWT_COOKIE_NAME = "jwt_token";
        private const string EMULATION_COOKIE_NAME = "jwt_emulation_token";
        private const string ORIGINAL_TOKEN_COOKIE_NAME = "jwt_original_token";

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

                // Generate authentication token
                var authResponse = _userService.Authenticate(new LoginRequest 
                { 
                    Username = request.Username, 
                    Password = request.Password 
                });

                if (authResponse == null)
                    return BadRequest(new { message = "Authentication failed after registration." });

                // Set JWT token in HTTP-only cookie
                Response.Cookies.Append("jwt_token", authResponse.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddYears(1)
                });

                var userResponse = new BasicUserResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = role.RoleName
                };

                return Ok(new { 
                    user = userResponse,
                    token = authResponse.Token
                });
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

            // Set JWT token in HTTP-only cookie
            Response.Cookies.Append("jwt_token", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddYears(1)
            });

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
            // Remove the JWT cookie
            Response.Cookies.Delete("jwt_token");

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
            var userResponse = new BasicUserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = role.RoleName
            };

            // Return the logout success message along with user information
            return Ok(new
            {
                message = "Logout successful.",
                user = userResponse
            });
        }

        // POST: /api/auth/emulate/{userId}
        [HttpPost("emulate/{userId}")]
        [Authorize(Policy = "Administrator")]
        public IActionResult StartEmulation(int userId)
        {
            // Get the target user
            var targetUser = _userRepository.GetUserById(userId);
            if (targetUser == null)
                return NotFound(new { message = "User not found" });

            // Get current admin's token - first check if we already have an original token stored
            var originalToken = Request.Cookies[ORIGINAL_TOKEN_COOKIE_NAME] ?? Request.Cookies[JWT_COOKIE_NAME];
            if (string.IsNullOrEmpty(originalToken))
                return Unauthorized(new { message = "No authentication token found" });

            // Generate new token for emulated user
            var emulationResponse = _userService.Authenticate(new LoginRequest 
            { 
                Username = targetUser.Username,
                IsEmulation = true
            });

            if (emulationResponse == null)
                return BadRequest(new { message = "Failed to generate emulation token" });

            // Store original admin token if not already stored
            if (Request.Cookies[ORIGINAL_TOKEN_COOKIE_NAME] == null)
            {
                Response.Cookies.Append(ORIGINAL_TOKEN_COOKIE_NAME, originalToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(24)
                });
            }

            // Set emulation token
            Response.Cookies.Append(JWT_COOKIE_NAME, emulationResponse.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(24)
            });

            return Ok(new { 
                message = $"Now emulating user: {targetUser.Username}",
                token = emulationResponse.Token
            });
        }

        // POST: /api/auth/stop-emulation
        [HttpPost("stop-emulation")]
        [Authorize]
        public IActionResult StopEmulation()
        {
            var originalToken = Request.Cookies[ORIGINAL_TOKEN_COOKIE_NAME];
            if (string.IsNullOrEmpty(originalToken))
                return BadRequest(new { message = "No emulation is currently active" });

            // Restore original token
            Response.Cookies.Append(JWT_COOKIE_NAME, originalToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddYears(1)
            });

            // Remove emulation cookies
            Response.Cookies.Delete(ORIGINAL_TOKEN_COOKIE_NAME);

            return Ok(new { 
                message = "Emulation stopped, returned to original user",
                token = originalToken
            });
        }

        // GET: /api/auth/profile
        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID." });
            }

            var user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
            {
                return BadRequest(new { message = "User role not found." });
            }

            // Viewing own profile always shows full information
            var response = AdminUserResponse.FromUser(user, role.RoleName);
            return Ok(new { user = response });
        }
    }
}
