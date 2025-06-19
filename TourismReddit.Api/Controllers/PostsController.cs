using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismReddit.Api.Data;
using TourismReddit.Api.Models;
using TourismReddit.Api.Dtos;

namespace TourismReddit.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out var id))
            {
                userId = id;
                return true;
            }
            _logger.LogWarning("Could not parse User ID from claims in CommentsController.");
            return false;
        }
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public PostsController(ApplicationDbContext context, ILogger<PostsController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PostDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPosts(
            [FromQuery] string sortBy = "new",
            [FromQuery] int limit = 25,
            [FromQuery] string? countryCode = null)
        {
            _logger.LogInformation("Getting posts: sortBy={SortBy}, limit={Limit}, countryCode={CountryCode}", sortBy, limit, countryCode);

            var query = _context.Posts.AsNoTracking();

            query = query
                .Where(p => !p.IsDeleted && p.Author != null && p.Author.IsActive)
                .Include(p => p.Author)
                .Include(p => p.Country)
                .Include(p => p.Votes)
                .Include(p => p.PostCategories)
                    .ThenInclude(pc => pc.Category);

            query = query
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag);

            if (!string.IsNullOrEmpty(countryCode))
            {
                query = query.Where(p => p.Country != null && p.Country.Code.ToLower() == countryCode.ToLower());
            }

            IQueryable<Post> orderedQuery;
            if (sortBy.ToLower() == "top")
            {
                orderedQuery = query.OrderByDescending(p => p.Votes.Sum(v => (int?)v.VoteType ?? 0))
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
                    AuthorUsername = p.Author != null ? p.Author.Username : "Unknown",
                    CountryName = p.Country != null ? p.Country.Name : "Unknown",
                    CountryCode = p.Country != null ? p.Country.Code : null,
                    CreatedAt = p.CreatedAt,
                    Score = p.Votes.Any() ? p.Votes.Sum(v => v.VoteType) : 0,
                    CategoryNames = p.PostCategories.Select(pc => pc.Category.Name).ToList(),
                    TagNames = p.PostTags.Select(pt => pt.Tag.Name).ToList(),
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDto>> GetPostById(int id)
        {
            _logger.LogInformation("Getting post by ID: {PostId}", id);

            var post = await _context.Posts
               .AsNoTracking()
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

            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = (post.Body is null) ? "No content" : post.Body,
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

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Creating post titled: {Title}", createPostDto.Title);

            string postBody = createPostDto.Body;

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Invalid user ID.");

            var countryExists = await _context.Countries.AnyAsync(c => c.Id == createPostDto.CountryId);
            if (!countryExists)
            {
                ModelState.AddModelError(nameof(createPostDto.CountryId), "Selected country does not exist.");
                return BadRequest(ModelState);
            }

            var post = new Post
            {
                Title = createPostDto.Title,
                Body = postBody,
                CountryId = createPostDto.CountryId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Latitude = createPostDto.Latitude,
                Longitude = createPostDto.Longitude,
            };

            if (createPostDto.CategoryIds?.Any() == true)
            {
                var validCategoryIds = await _context.Categories
                    .Where(c => createPostDto.CategoryIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();
                foreach (var catId in validCategoryIds)
                {
                    post.PostCategories.Add(new PostCategory { CategoryId = catId });
                }
            }

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
            await _context.SaveChangesAsync();

            await _context.Entry(post).Reference(p => p.Author).LoadAsync();
            await _context.Entry(post).Reference(p => p.Country).LoadAsync();
            await _context.Entry(post).Collection(p => p.PostCategories).Query().Include(pc => pc.Category).LoadAsync();
            await _context.Entry(post).Collection(p => p.PostTags).Query().Include(pt => pt.Tag).LoadAsync();

            var postDto = new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                AuthorUsername = post.Author?.Username ?? "Unknown",
                CountryName = post.Country?.Name ?? "Unknown",
                CountryCode = post.Country?.Code,
                CreatedAt = post.CreatedAt,
                Score = 0,
                CategoryNames = post.PostCategories.Select(pc => pc.Category.Name).ToList(),
                TagNames = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                Latitude = post.Latitude,
                Longitude = post.Longitude,
                ImageUrl = post.ImageUrl
            };

            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, postDto);
        }

        [HttpPost("{postId}/vote")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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

        [HttpDelete("{postId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePost(int postId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            _logger.LogWarning("[REDACTION] Attempting redaction of Post ID: {PostId} by User ID: {UserId}", postId, userId);

            var post = await _context.Posts
                .Include(p => p.PostCategories)
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null) return NotFound("Post not found.");

            if (post.UserId != userId)
            {
                _logger.LogWarning("[REDACTION] User {UserId} forbidden to redact Post ID: {PostId} owned by User ID: {OwnerId}", userId, postId, post.UserId);
                return Forbid();
            }

            post.IsDeleted = true;
            post.Body = "[REMOVED]";
            post.UserId = null;
            post.ImageUrl = null;
            post.Latitude = null;
            post.Longitude = null;

            post.PostCategories.Clear();
            post.PostTags.Clear();

            _context.Posts.Update(post);

            await _context.SaveChangesAsync();

            _logger.LogInformation("[REDACTION] Successfully redacted Post ID: {PostId}", postId);
            return NoContent();
        }
        [HttpPost("{postId}/image")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadPostImage(int postId, IFormFile imageFile)
        {
            _logger.LogInformation("Attempting image upload for Post ID: {PostId}", postId);

            if (imageFile == null || imageFile.Length == 0) return BadRequest("No image file provided.");
            if (imageFile.Length > 5 * 1024 * 1024) return BadRequest("Image file size exceeds the limit (e.g., 5MB).");
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension)) return BadRequest("Invalid image file type. Allowed: JPG, PNG, WEBP.");

            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound("Post not found.");
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId) || post.UserId != userId) return Forbid();

            try
            {
                var uploadsFolderName = "uploads";
                var postImagesFolderName = $"post_{postId}";
                var uploadsFolderPath = Path.Combine(_environment.ContentRootPath, uploadsFolderName);
                var postImagesFolderPath = Path.Combine(uploadsFolderPath, postImagesFolderName);
                Directory.CreateDirectory(postImagesFolderPath);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(postImagesFolderPath, uniqueFileName);
                _logger.LogInformation("Saving image for Post ID {PostId} to {FilePath}", postId, filePath);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var relativeImagePath = $"/{uploadsFolderName}/{postImagesFolderName}/{uniqueFileName}".Replace("\\", "/");
                post.ImageUrl = relativeImagePath;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved image for Post ID {PostId}. DB Path: {ImageUrl}", postId, relativeImagePath);
                return Ok(new { imageUrl = relativeImagePath });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for Post ID: {PostId}", postId);
                return StatusCode(500, "Internal server error during image upload.");
            }
        }
    }
}