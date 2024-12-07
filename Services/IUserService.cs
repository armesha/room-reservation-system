using RoomReservationSystem.Models;
using RoomReservationSystem.Models.Auth;

namespace RoomReservationSystem.Services
{
    public interface IUserService
    {
        RegisterResponse Register(RegisterRequest request);
        RegisterResponse AdminCreateUser(AdminUserCreateRequest request);
        LoginResponse Authenticate(LoginRequest request);
    }
}
