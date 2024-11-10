// Services/IUserService.cs
using RoomReservationSystem.Models;
using RoomReservationSystem.Models.Auth;

namespace RoomReservationSystem.Services
{
    public interface IUserService
    {
        RegisterResponse Register(RegisterRequest request);
        LoginResponse Authenticate(LoginRequest request);
    }
}
