using RoomReservationSystem.Models;
using System.Collections.Generic;

namespace RoomReservationSystem.Repositories
{
    public interface ICountryRepository
    {
        IEnumerable<Country> GetAllCountries();
        Country GetCountryByCode(string code);
        void SetUserCountry(int userId, string countryCode);
        string GetUserCountryCode(int userId);
    }
}
