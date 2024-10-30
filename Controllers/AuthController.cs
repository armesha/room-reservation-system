//Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomReservationSystem.Data;
using RoomReservationSystem.DTOs.Auth;
using RoomReservationSystem.Models;
using RoomReservationSystem.Services;
using System.Security.Cryptography;
using System.Text;

namespace RoomReservationSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email is already registered." });
            }

            // Hash the password
            var passwordHash = ComputeSha256Hash(request.Password);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "User" // Default role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new RegisterResponse { Message = "User registered successfully." });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var passwordHash = ComputeSha256Hash(request.Password);
            if (user.PasswordHash != passwordHash)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var token = _tokenService.GenerateToken(user);
            return Ok(new LoginResponse
            {
                Token = token,
                Role = user.Role
            });
        }

        // Utility method to compute SHA256 hash
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                foreach (var t in bytes)
                {
                    builder.Append(t.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
