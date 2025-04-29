using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TourismReddit.Api.Data;
using TourismReddit.Api.Models;

namespace TourismReddit.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CountriesController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}