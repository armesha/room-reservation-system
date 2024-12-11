using Microsoft.AspNetCore.Mvc;
using RoomReservationSystem.Models;
using RoomReservationSystem.Repositories;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System;

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

        // POST: api/countries
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult<Country> CreateCountry([FromBody] Country country)
        {
            try
            {
                var newCountry = _countryRepository.AddCountry(country);
                return CreatedAtAction(nameof(GetAllCountries), new { id = newCountry.CountryId }, newCountry);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the country", error = ex.Message });
            }
        }

        // PUT: api/countries/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult UpdateCountry(int id, [FromBody] Country country)
        {
            if (id != country.CountryId)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            try
            {
                _countryRepository.UpdateCountry(country);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the country", error = ex.Message });
            }
        }

        // DELETE: api/countries/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteCountry(int id)
        {
            try
            {
                _countryRepository.DeleteCountry(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the country", error = ex.Message });
            }
        }
    }
}
