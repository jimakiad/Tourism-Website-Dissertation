using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TourismReddit.Api.Data;
using TourismReddit.Api.Models;
using TourismReddit.Api.Dtos;      // <<< Ensure this using statement is present

namespace TourismReddit.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        // Optional: Inject a Markdown Sanitizer service here later
        // private readonly IMarkdownSanitizer _sanitizer;

        public PostsController(ApplicationDbContext context /*, IMarkdownSanitizer sanitizer */)
        {
            _context = context;
            // _sanitizer = sanitizer;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PostDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPosts([FromQuery] string sortBy = "new", [FromQuery] int limit = 25)
        {
            var query = _context.Posts
                .AsNoTracking() // Good practice for read-only queries
                .Include(p => p.Author)
                .Include(p => p.Country)
                .Include(p => p.Votes);

            IOrderedQueryable<Post> orderedQuery;
            if (sortBy.ToLower() == "top")
            {
                orderedQuery = query.OrderByDescending(p => p.Votes.Sum(v => v.VoteType))
                                    .ThenByDescending(p => p.CreatedAt);
            }
            else
            {
                orderedQuery = query.OrderByDescending(p => p.CreatedAt);
            }

            var posts = await orderedQuery
                .Take(limit)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body, // Return raw Markdown for now
                    AuthorUsername = p.Author != null ? p.Author.Username : "Unknown",
                    CountryName = p.Country != null ? p.Country.Name : "Unknown",
                    Tags = p.Tags,
                    Categories = p.Categories,
                    CreatedAt = p.CreatedAt,
                    Score = p.Votes.Any() ? p.Votes.Sum(v => v.VoteType) : 0
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreatePost(CreatePostDto createPostDto)
        {
             if (!ModelState.IsValid)
             {
                 return BadRequest(ModelState);
             }

            // --- PLACEHOLDER FOR MARKDOWN SANITIZATION ---
            // string sanitizedBody = _sanitizer.Sanitize(createPostDto.Body); // Implement this later!
            string unsafeBody = createPostDto.Body; // Using raw input for now - NOT PRODUCTION SAFE
            // --------------------------------------------

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            var countryExists = await _context.Countries.AnyAsync(c => c.Id == createPostDto.CountryId);
            if (!countryExists)
            {
                // Add specific error to ModelState
                 ModelState.AddModelError(nameof(createPostDto.CountryId), "Selected country does not exist.");
                 return BadRequest(ModelState);
            }

            var post = new Post
            {
                Title = createPostDto.Title,
                Body = unsafeBody, // Use sanitizedBody in production
                CountryId = createPostDto.CountryId,
                Tags = createPostDto.Tags ?? string.Empty,
                Categories = createPostDto.Categories ?? string.Empty,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Reload necessary related data for the response
            await _context.Entry(post).Reference(p => p.Author).LoadAsync();
            await _context.Entry(post).Reference(p => p.Country).LoadAsync();

            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                AuthorUsername = post.Author?.Username ?? "Unknown",
                CountryName = post.Country?.Name ?? "Unknown",
                Tags = post.Tags,
                Categories = post.Categories,
                CreatedAt = post.CreatedAt,
                Score = 0 // New post score
            };

            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, postDto);
        }


        [HttpPost("{postId}/vote")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Return { score: newScore }
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VotePost(int postId, VoteDto voteDto)
        {
             if (!ModelState.IsValid)
             {
                 return BadRequest(ModelState);
             }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user ID in token.");
            }

            // Find the specific vote by this user for this post
            var existingVote = await _context.Votes
                                         .FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteDto.Direction)
                {
                    _context.Votes.Remove(existingVote); // Unvote
                }
                else
                {
                    existingVote.VoteType = voteDto.Direction; // Change vote
                    _context.Votes.Update(existingVote);
                }
            }
            else
            {
                // Need to check if post exists before adding a new vote
                var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
                if (!postExists)
                {
                    return NotFound("Post not found.");
                }

                var newVote = new Vote
                {
                    UserId = userId,
                    PostId = postId,
                    VoteType = voteDto.Direction
                };
                _context.Votes.Add(newVote); // New vote
            }

            await _context.SaveChangesAsync();

            var newScore = await _context.Votes
                                       .Where(v => v.PostId == postId)
                                       .SumAsync(v => (int?)v.VoteType) ?? 0; // Sum nullable int

            return Ok(new { score = newScore });
        }

        // Helper endpoint used by CreatePost's CreatedAtAction
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDto>> GetPostById(int id)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Include(p => p.Author)
                .Include(p => p.Country)
                .Include(p => p.Votes)
                .Where(p => p.Id == id)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    AuthorUsername = p.Author != null ? p.Author.Username : "Unknown",
                    CountryName = p.Country != null ? p.Country.Name : "Unknown",
                    Tags = p.Tags,
                    Categories = p.Categories,
                    CreatedAt = p.CreatedAt,
                    Score = p.Votes.Any() ? p.Votes.Sum(v => v.VoteType) : 0
                })
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }
    }
}