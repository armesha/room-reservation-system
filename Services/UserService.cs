// Services/UserService.cs
using RoomReservationSystem.Models;
using RoomReservationSystem.Models.Auth;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.Utilities;
using System;

namespace RoomReservationSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly JwtTokenGenerator _tokenGenerator;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository, JwtTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _tokenGenerator = tokenGenerator;
        }

        public RegisterResponse Register(RegisterRequest request)
        {
            // Check if username or email already exists
            var existingUser = _userRepository.GetUserByUsername(request.Username);
            if (existingUser != null)
            {
                return new RegisterResponse { Success = false, Message = "Username already exists." };
            }

            // Assign 'Registered User' role by default
            var role = _roleRepository.GetRoleByName("Registered User");
            if (role == null)
            {
                return new RegisterResponse { Success = false, Message = "User role not found." };
            }

            // Hash password
            var hashedPassword = PasswordHasher.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Username = request.Username,
                PasswordHash = hashedPassword,
                Email = request.Email,
                RoleId = role.RoleId,
                RegistrationDate = DateTime.UtcNow
            };

            _userRepository.AddUser(user);

            return new RegisterResponse { Success = true, Message = "Registration successful." };
        }

        public LoginResponse Authenticate(LoginRequest request)
        {
            var user = _userRepository.GetUserByUsername(request.Username);
            if (user == null)
                return null;

            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return null;

            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
                return null;

            var loginResponse = new LoginResponse
            {
                Username = user.Username,
                Role = role.RoleName,
                UserId = user.UserId // Populate UserId
            };

            // Generate JWT Token
            loginResponse.Token = _tokenGenerator.GenerateToken(loginResponse);

            return loginResponse;
        }
    }
}
