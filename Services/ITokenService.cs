//Services/ITokenService.cs
using RoomReservationSystem.Models;

namespace RoomReservationSystem.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
