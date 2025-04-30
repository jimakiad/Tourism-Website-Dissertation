using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismReddit.Api.Data;
using TourismReddit.Api.Models;

namespace TourismReddit.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(ApplicationDbContext context, ILogger<CountriesController> logger) // Inject logger
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Country>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
        {
            var countries = await _context.Countries
                                        .AsNoTracking()
                                        .OrderBy(c => c.Name)
                                        .ToListAsync();
            return Ok(countries);
        }

        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(Country), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Country>> GetCountryByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest("Country code cannot be empty.");
            }

            _logger.LogInformation("Getting country by code: {Code}", code);

            // Find country by code (case-insensitive)
            var country = await _context.Countries
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());

            if (country == null)
            {
                _logger.LogWarning("Country with code {Code} not found.", code);
                return NotFound("Country not found.");
            }

            // Return the whole Country object for now (including ID, Name, Code)
            // Later, you might add Description/Rules here
            return Ok(country);
        }
    }
}