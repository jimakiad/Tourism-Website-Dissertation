// Controllers/NewsletterController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TourismReddit.Api.Data;

namespace TourismReddit.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // All actions require login
public class NewsletterController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(ApplicationDbContext context, ILogger<NewsletterController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Helper to get current user ID (could be moved to a base controller)
    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out var id))
        {
            userId = id;
            return true;
        }
        _logger.LogWarning("Could not parse User ID from claims.");
        return false;
    }

    // POST /api/newsletter/subscribe
    [HttpPost("subscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Subscribe()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found.");

        if (!user.IsSubscribed) // Only update if not already subscribed
        {
            user.IsSubscribed = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} subscribed to newsletter.", userId);
        } else {
             _logger.LogInformation("User {UserId} attempted to subscribe but was already subscribed.", userId);
        }

        return Ok(new { message = "Subscription successful." });
    }

    // POST /api/newsletter/unsubscribe
    [HttpPost("unsubscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unsubscribe()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found.");

         if (user.IsSubscribed) // Only update if currently subscribed
        {
            user.IsSubscribed = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} unsubscribed from newsletter.", userId);
        } else {
             _logger.LogInformation("User {UserId} attempted to unsubscribe but was already unsubscribed.", userId);
        }


        return Ok(new { message = "Unsubscription successful." });
    }

     // GET /api/newsletter/status (Optional: If needed separately from login token)
     [HttpGet("status")]
     [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
     public async Task<IActionResult> GetStatus()
     {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found.");

        return Ok(new { isSubscribed = user.IsSubscribed });
     }
}