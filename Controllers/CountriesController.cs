using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;

namespace RoomReservationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryRepository _countryRepository;

        public CountriesController(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        // GET: api/countries
        [HttpGet]
        public ActionResult<IEnumerable<Country>> GetAllCountries()
        {
            var countries = _countryRepository.GetAllCountries();
            return Ok(new { countries = countries });
        }
    }
}
