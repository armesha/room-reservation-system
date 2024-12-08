using RoomReservationSystem.Models;
using RoomReservationSystem.Models.Auth;
using RoomReservationSystem.Repositories;
using RoomReservationSystem.Utilities;
using System;
using System.Collections.Generic;

namespace RoomReservationSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly JwtTokenGenerator _tokenGenerator;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository, 
            JwtTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _tokenGenerator = tokenGenerator;
        }

        public RegisterResponse Register(RegisterRequest request)
        {
            // Check if username already exists
            var existingUser = _userRepository.GetUserByUsername(request.Username);
            if (existingUser != null)
            {
                return new RegisterResponse { Success = false, Message = "Username already exists." };
            }

            // Check if email already exists
            existingUser = _userRepository.GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse { Success = false, Message = "Email already exists." };
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

            // Generate token for the new user
            var loginResponse = new LoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = "Registered User"
            };
            loginResponse.Token = _tokenGenerator.GenerateToken(loginResponse);

            return new RegisterResponse 
            { 
                Success = true,
                Message = "Registration successful."
            };
        }

        public RegisterResponse AdminCreateUser(AdminUserCreateRequest request)
        {
            // Check if username or email already exists
            var existingUser = _userRepository.GetUserByUsername(request.Username);
            if (existingUser != null)
            {
                return new RegisterResponse { Success = false, Message = "Username already exists." };
            }

            // Get role by name
            var role = _roleRepository.GetRoleByName(request.RoleName);
            if (role == null)
            {
                return new RegisterResponse { Success = false, Message = "Invalid role name." };
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

            return new RegisterResponse { Success = true, Message = "User created successfully." };
        }

        public LoginResponse Authenticate(LoginRequest request)
        {
            var user = _userRepository.GetUserByUsername(request.Username);
            if (user == null)
                return null;

            // Skip password verification for emulation requests
            if (!request.IsEmulation && !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return null;

            var role = _roleRepository.GetRoleById(user.RoleId);
            if (role == null)
                return null;

            var loginResponse = new LoginResponse
            {
                Username = user.Username,
                Role = role.RoleName,
                UserId = user.UserId,
                IsEmulated = request.IsEmulation
            };

            // Generate JWT Token
            loginResponse.Token = _tokenGenerator.GenerateToken(loginResponse);

            return loginResponse;
        }

        public User UpdateUser(int userId, UpdateUserRequest request)
        {
            var user = _userRepository.GetUserById(userId);
            if (user == null)
                return null;

            // Update user properties if provided
            if (!string.IsNullOrEmpty(request.Username))
                user.Username = request.Username;
            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;
            if (request.RoleId.HasValue)
                user.RoleId = request.RoleId.Value;
            if (!string.IsNullOrEmpty(request.Name))
                user.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Surname))
                user.Surname = request.Surname;
            if (!string.IsNullOrEmpty(request.Phone))
                user.Phone = request.Phone;
            if (!string.IsNullOrEmpty(request.Code))
                user.Code = request.Code;

            return _userRepository.UpdateUser(user);
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = _userRepository.GetUserById(userId);
            if (user == null)
                return false;

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return false;

            // Hash new password and update
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            var updatedUser = _userRepository.UpdateUser(user);
            return updatedUser != null;
        }

        public User GetUserByUsername(string username)
        {
            return _userRepository.GetUserByUsername(username);
        }

        public void DeleteUser(int userId)
        {
            _userRepository.DeleteUser(userId);
        }

        public void AddUser(User user)
        {
            _userRepository.AddUser(user);
        }

        public IEnumerable<User> GetPaginatedUsers(UserFilterParameters parameters)
        {
            return _userRepository.GetPaginatedUsers(parameters);
        }

        public int GetTotalUsersCount()
        {
            return _userRepository.GetTotalUsersCount();
        }

        public User GetUserById(int userId)
        {
            return _userRepository.GetUserById(userId);
        }
    }
}
