// Controllers/PostsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TourismReddit.Api.Data;
using TourismReddit.Api.Models;
using TourismReddit.Api.Dtos;
using Microsoft.AspNetCore.Http; // Required for IFormFile
using System.IO; // Required for Path
using Microsoft.Extensions.Hosting; // Required for IWebHostEnvironment (use this instead of IHostingEnvironment in .NET Core 3+)

namespace TourismReddit.Api.Controllers
{
    [Route("api/[controller]")] // Using [controller] token is conventional
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostsController> _logger;
        private readonly IWebHostEnvironment _environment; // Use IWebHostEnvironment

        public PostsController(ApplicationDbContext context, ILogger<PostsController> logger, IWebHostEnvironment environment) // Inject IWebHostEnvironment
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // GET: /api/posts
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PostDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPosts(
            [FromQuery] string sortBy = "new",
            [FromQuery] int limit = 25,
            [FromQuery] string? countryCode = null)
        {
            _logger.LogInformation("Getting posts: sortBy={SortBy}, limit={Limit}, countryCode={CountryCode}", sortBy, limit, countryCode);

            // Start base query
            var query = _context.Posts.AsNoTracking();

            // Apply includes using separate statements for clarity and type safety
            query = query
                .Include(p => p.Author)
                .Include(p => p.Country)
                .Include(p => p.Votes)
                .Include(p => p.PostCategories)
                    .ThenInclude(pc => pc.Category);

            // Chain the next include from the result of the previous one
            query = query
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag);

            // Apply Country Filter
            if (!string.IsNullOrEmpty(countryCode))
            {
                // Ensure case-insensitive comparison if needed by DB collation
                query = query.Where(p => p.Country != null && p.Country.Code.ToLower() == countryCode.ToLower());
            }

            // Apply Sorting
            IQueryable<Post> orderedQuery; // Use IQueryable here
            if (sortBy.ToLower() == "top")
            {
                // Order by score (sum of votes)
                 orderedQuery = query.OrderByDescending(p => p.Votes.Sum(v => (int?)v.VoteType ?? 0)) // Handle potential null sum or no votes
                                     .ThenByDescending(p => p.CreatedAt);
            }
            else // Default to 'new'
            {
                orderedQuery = query.OrderByDescending(p => p.CreatedAt);
            }

            // Apply Limit and Project to DTO
            var posts = await orderedQuery // Use the ordered query
                .Take(limit)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    // Body = p.Body, // Optionally omit for list view
                    AuthorUsername = p.Author != null ? p.Author.Username : "Unknown",
                    CountryName = p.Country != null ? p.Country.Name : "Unknown",
                    CountryCode = p.Country != null ? p.Country.Code : null,
                    CreatedAt = p.CreatedAt,
                    Score = p.Votes.Any() ? p.Votes.Sum(v => v.VoteType) : 0, // Calculate score in projection
                    CategoryNames = p.PostCategories.Select(pc => pc.Category.Name).ToList(),
                    TagNames = p.PostTags.Select(pt => pt.Tag.Name).ToList(),
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Ok(posts);
        }

        // GET: /api/posts/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDto>> GetPostById(int id)
        {
            _logger.LogInformation("Getting post by ID: {PostId}", id);

            // Fetch the single post with all necessary related data
            // No need to include Comments here if fetched separately by frontend
            var post = await _context.Posts
               .AsNoTracking() // Good for read-only scenarios
               .Include(p => p.Author)
               .Include(p => p.Country)
               .Include(p => p.Votes)
               .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
               .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
               .Where(p => p.Id == id)
               .FirstOrDefaultAsync();

            if (post == null)
            {
                _logger.LogWarning("Post with ID {PostId} not found.", id);
                return NotFound();
            }

            // Map the retrieved entity to the DTO
            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body, // Include full body for detail view
                AuthorUsername = post.Author?.Username ?? "Unknown",
                CountryName = post.Country?.Name ?? "Unknown",
                CountryCode = post.Country?.Code,
                CreatedAt = post.CreatedAt,
                Score = post.Votes.Any() ? post.Votes.Sum(v => v.VoteType) : 0,
                CategoryNames = post.PostCategories.Select(pc => pc.Category.Name).ToList(),
                TagNames = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                Latitude = post.Latitude,
                Longitude = post.Longitude,
                ImageUrl = post.ImageUrl,
            };

            return Ok(postDto);
        }


        // POST: /api/posts
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Added for country check
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto) // Added [FromBody]
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Creating post titled: {Title}", createPostDto.Title);

            // --- Consider Sanitization for Body ---
            string postBody = createPostDto.Body; // Replace with sanitized version later
            // ---

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Invalid user ID.");

            // Verify country exists
            var countryExists = await _context.Countries.AnyAsync(c => c.Id == createPostDto.CountryId);
            if (!countryExists)
            {
                 ModelState.AddModelError(nameof(createPostDto.CountryId), "Selected country does not exist.");
                 return BadRequest(ModelState);
            }

            // Create the main Post entity
            var post = new Post
            {
                Title = createPostDto.Title,
                Body = postBody, // Use sanitized body
                CountryId = createPostDto.CountryId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Latitude = createPostDto.Latitude,
                Longitude = createPostDto.Longitude,
                // ImageUrl is set after upload
            };

            // Add selected Categories
            if (createPostDto.CategoryIds?.Any() == true)
            {
                // Verify IDs exist to prevent errors (optional but recommended)
                var validCategoryIds = await _context.Categories
                    .Where(c => createPostDto.CategoryIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();
                foreach (var catId in validCategoryIds)
                {
                    post.PostCategories.Add(new PostCategory { CategoryId = catId });
                }
                 // Optional: Check if any requested IDs were invalid and report back?
            }

            // Add selected Tags
            if (createPostDto.TagIds?.Any() == true)
            {
                 var validTagIds = await _context.Tags
                    .Where(t => createPostDto.TagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();
                foreach (var tagId in validTagIds)
                {
                    post.PostTags.Add(new PostTag { TagId = tagId });
                }
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); // Save post FIRST to get its ID

            // --- Load related data necessary for the response DTO ---
            // Use context tracking or reload manually if needed
            await _context.Entry(post).Reference(p => p.Author).LoadAsync();
            await _context.Entry(post).Reference(p => p.Country).LoadAsync();
            // Reload collections to get Category/Tag names if not implicitly loaded
            await _context.Entry(post).Collection(p => p.PostCategories).Query().Include(pc => pc.Category).LoadAsync();
            await _context.Entry(post).Collection(p => p.PostTags).Query().Include(pt => pt.Tag).LoadAsync();
            // ---

            // Map to DTO for the response
            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                AuthorUsername = post.Author?.Username ?? "Unknown",
                CountryName = post.Country?.Name ?? "Unknown",
                CountryCode = post.Country?.Code,
                CreatedAt = post.CreatedAt,
                Score = 0, // New post has 0 score
                CategoryNames = post.PostCategories.Select(pc => pc.Category.Name).ToList(),
                TagNames = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                Latitude = post.Latitude,
                Longitude = post.Longitude,
                ImageUrl = post.ImageUrl // Will be null initially
            };

            // Return 201 Created with the location of the new resource and the resource itself
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, postDto);
        }


        // POST: /api/posts/{postId}/vote
        [HttpPost("{postId}/vote")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Returns { score: newScore }
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VotePost(int postId, [FromBody] VoteDto voteDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Voting on Post ID: {PostId}, Direction: {Direction}", postId, voteDto.Direction);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Invalid user ID.");

            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteDto.Direction)
                {
                    _logger.LogInformation("User {UserId} unvoting Post ID: {PostId}", userId, postId);
                    _context.Votes.Remove(existingVote);
                }
                else
                {
                     _logger.LogInformation("User {UserId} changing vote on Post ID: {PostId} to {Direction}", userId, postId, voteDto.Direction);
                    existingVote.VoteType = voteDto.Direction;
                    _context.Votes.Update(existingVote);
                }
            }
            else
            {
                var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
                if (!postExists) return NotFound("Post not found.");

                _logger.LogInformation("User {UserId} casting new vote on Post ID: {PostId} with direction {Direction}", userId, postId, voteDto.Direction);
                var newVote = new Vote { UserId = userId, PostId = postId, VoteType = voteDto.Direction };
                _context.Votes.Add(newVote);
            }

            await _context.SaveChangesAsync();

            var newScore = await _context.Votes
                .Where(v => v.PostId == postId)
                .SumAsync(v => (int?)v.VoteType) ?? 0;

            _logger.LogInformation("Vote processed for Post ID: {PostId}. New score: {NewScore}", postId, newScore);

            return Ok(new { score = newScore });
        }


        // POST: /api/posts/{postId}/image
        [HttpPost("{postId}/image")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Returns { imageUrl: "path" }
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadPostImage(int postId, IFormFile imageFile)
        {
            _logger.LogInformation("Attempting image upload for Post ID: {PostId}", postId);

            // --- Input Validation ---
            if (imageFile == null || imageFile.Length == 0) return BadRequest("No image file provided.");
            if (imageFile.Length > 5 * 1024 * 1024) return BadRequest("Image file size exceeds the limit (e.g., 5MB)."); // Example limit
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension)) return BadRequest("Invalid image file type. Allowed: JPG, PNG, WEBP.");
            // --- End Validation ---

            // Find Post and Verify Ownership
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound("Post not found.");
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId) || post.UserId != userId) return Forbid();

            try
            {
                // Define Paths
                var uploadsFolderName = "uploads";
                var postImagesFolderName = $"post_{postId}";
                // Use ContentRootPath for saving backend files
                var uploadsFolderPath = Path.Combine(_environment.ContentRootPath, uploadsFolderName);
                var postImagesFolderPath = Path.Combine(uploadsFolderPath, postImagesFolderName);
                Directory.CreateDirectory(postImagesFolderPath); // Ensure directory exists

                // Save File
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(postImagesFolderPath, uniqueFileName);
                _logger.LogInformation("Saving image for Post ID {PostId} to {FilePath}", postId, filePath);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Update Database with Relative URL Path (matches static file config)
                // IMPORTANT: Ensure this path structure matches UseStaticFiles RequestPath
                var relativeImagePath = $"/{uploadsFolderName}/{postImagesFolderName}/{uniqueFileName}".Replace("\\", "/");
                post.ImageUrl = relativeImagePath;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved image for Post ID {PostId}. DB Path: {ImageUrl}", postId, relativeImagePath);
                return Ok(new { imageUrl = relativeImagePath }); // Return the relative path

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for Post ID: {PostId}", postId);
                return StatusCode(500, "Internal server error during image upload.");
            }
        }
    }
}