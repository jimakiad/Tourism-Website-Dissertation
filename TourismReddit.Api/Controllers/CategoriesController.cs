using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourismReddit.Api.Data;
using TourismReddit.Api.Dtos;

namespace TourismReddit.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public CategoriesController(ApplicationDbContext context) { _context = context; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }
}