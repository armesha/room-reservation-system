using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models.Auth;
using RoomReservationSystem.Services;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            var response = _userService.Register(request);
            if (response.Success)
                return Ok(new { success = true, message = response.Message });

            return BadRequest(new { success = false, message = response.Message });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            var response = _userService.Authenticate(request);
            if (response == null)
                return Unauthorized(new { success = false, message = "Invalid credentials." });

            // Set JWT token in cookie
            Response.Cookies.Append("jwt", response.Token, new CookieOptions
            {
                HttpOnly = true,
            });

            return Ok(new { success = true, data = new { response.Username, response.Role, response.UserId } });
        }

        // POST: /api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Since JWT is stateless, implement logout on client side by discarding the token
            return Ok(new { success = true, message = "Logout successful." });
        }
    }
}
