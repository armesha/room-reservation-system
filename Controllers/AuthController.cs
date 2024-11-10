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
                return BadRequest(ModelState);

            var response = _userService.Register(request);
            if (response.Success)
                return Ok(new { message = response.Message });

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

            return Ok(response);
        }

        // POST: /api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful." });
        }
    }
}
