using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismReddit.Api.Data;
using TourismReddit.Api.Dtos;
using TourismReddit.Api.Models;

namespace TourismReddit.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out var id))
        {
            userId = id;
            return true;
        }
        _logger.LogWarning("Could not parse User ID from claims in UsersController.");
        return false;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var user = await _context.Users
                               .AsNoTracking()
                               .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("User {UserId} not found or inactive when fetching profile.", userId);
            return NotFound("User not found.");
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsSubscribed = user.IsSubscribed
        };

        return Ok(userDto);
    }

    [HttpGet("me/posts")]
    [ProducesResponseType(typeof(IEnumerable<PostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
     public async Task<ActionResult<IEnumerable<PostDto>>> GetCurrentUserPosts(
        [FromQuery] string sortBy = "new",
        [FromQuery] int limit = 50)
     {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        _logger.LogInformation("Fetching posts for current user: {UserId}", userId);

        var query = _context.Posts
            .Where(p => p.UserId == userId)
            .AsNoTracking()
            .Include(p => p.Country)
            .Include(p => p.Votes)
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag);

         IQueryable<Post> orderedQuery;
         if (sortBy.ToLower() == "top") {
             orderedQuery = query.OrderByDescending(p => p.Votes.Sum(v => (int?)v.VoteType ?? 0))
                                 .ThenByDescending(p => p.CreatedAt);
         } else {
             orderedQuery = query.OrderByDescending(p => p.CreatedAt);
         }

         var posts = await orderedQuery
            .Take(limit)
            .Select(p => new PostDto {
                 Id = p.Id,
                 Title = p.Title,
                 Body = p.IsDeleted ? "[REMOVED]" : (p.Body ?? string.Empty),
                 AuthorUsername = "[My Posts]",
                 CountryName = p.Country != null ? p.Country.Name : "Unknown",
                 CountryCode = p.Country != null ? p.Country.Code : null,
                 CreatedAt = p.CreatedAt,
                 Score = p.Votes.Any() ? p.Votes.Sum(v => v.VoteType) : 0,
                 CategoryNames = !p.IsDeleted ? p.PostCategories.Select(pc => pc.Category.Name).ToList() : new List<string>(),
                 TagNames = !p.IsDeleted ? p.PostTags.Select(pt => pt.Tag.Name).ToList() : new List<string>(),
                 Latitude = !p.IsDeleted ? p.Latitude : null,
                 Longitude = !p.IsDeleted ? p.Longitude : null,
                 ImageUrl = !p.IsDeleted ? p.ImageUrl : null
                 
             })
            .ToListAsync();

         return Ok(posts);
     }


    [HttpGet("me/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
     public async Task<ActionResult<IEnumerable<CommentDto>>> GetCurrentUserComments(
        [FromQuery] string sortBy = "new",
        [FromQuery] int limit = 50)
     {
         if (!TryGetUserId(out var userId)) return Unauthorized();

         _logger.LogInformation("Fetching comments for current user: {UserId}", userId);

          var query = _context.Comments
            .Where(c => c.UserId == userId)
            .AsNoTracking()
            .Include(c => c.Post)
                .ThenInclude(p => p.Author)
            .Include(c => c.CommentVotes);

          IQueryable<Comment> orderedQuery;
          if (sortBy.ToLower() == "score") {
              orderedQuery = query.OrderByDescending(c => c.CommentVotes.Sum(v => (int?)v.VoteType ?? 0))
                                  .ThenByDescending(c => c.CreatedAt);
          } else {
              orderedQuery = query.OrderByDescending(c => c.CreatedAt);
          }

           var comments = await orderedQuery
             .Take(limit)
             .Select(c => new CommentDto {
                 Id = c.Id,
                 Body = c.IsDeleted ? "[REMOVED]" : (c.Body ?? string.Empty),
                 UserId = c.UserId ?? 0, 
                 AuthorUsername = "[My Comment]", 
                 PostId = c.PostId,
                 ParentCommentId = c.ParentCommentId,
                 CreatedAt = c.CreatedAt,
                 Score = c.CommentVotes.Any() ? c.CommentVotes.Sum(v => v.VoteType) : 0,
             })
             .ToListAsync();

          return Ok(comments);
     }


    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCurrentUserAccount()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

         _logger.LogWarning("[DEACTIVATION] Attempting account deactivation for User ID: {UserId}", userId);

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
             _logger.LogWarning("User {UserId} not found for account deactivation.", userId);
            return NotFound("User not found.");
        }

        user.IsActive = false;
        user.Username = $"[DELETED_{user.Id}_{Guid.NewGuid().ToString().Substring(0, 8)}]";
        user.Email = $"{user.Id}@deleted.local";
        user.PasswordHash = "";
        user.IsSubscribed = false;

        _context.Users.Update(user);
        try
        {
             await _context.SaveChangesAsync();
             _logger.LogInformation("[DEACTIVATION] Successfully deactivated account for User ID: {UserId}", userId);
             return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error deactivating account for User ID: {UserId}. Potential unique constraint violation.", userId);
            return StatusCode(500, "An error occurred while deactivating the account. It's possible the generated redacted username/email conflicted.");
        }
         catch (Exception ex)
        {
             _logger.LogError(ex, "Generic error deactivating account for User ID: {UserId}", userId);
            return StatusCode(500, "An error occurred while deactivating the account.");
        }
    }
}