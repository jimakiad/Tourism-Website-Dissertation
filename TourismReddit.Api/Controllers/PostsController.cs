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

namespace TourismReddit.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PostsController> _logger; // Added logger

        // --- Inject IWebHostEnvironment ---
        private readonly IWebHostEnvironment _environment;

        public PostsController(ApplicationDbContext context, ILogger<PostsController> logger, IWebHostEnvironment environment) // Add environment
        {
            _context = context;
            _logger = logger;
            _environment = environment; // Assign environment
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PostDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPosts([FromQuery] string sortBy = "new", [FromQuery] int limit = 25)
        {
            _logger.LogInformation("Getting posts: sortBy={SortBy}, limit={Limit}", sortBy, limit); // Log entry

            var query = _context.Posts
                .AsNoTracking()
                .Include(p => p.Author)
                .Include(p => p.Country)
                .Include(p => p.Votes)
                // --- Include Categories/Tags ---
                .Include(p => p.PostCategories)
                    .ThenInclude(pc => pc.Category) // Include Category info via join table
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag);      // Include Tag info via join table
                                                     // --- End Include ---

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
                    Body = p.Body,
                    AuthorUsername = p.Author != null ? p.Author.Username : "Unknown",
                    CountryName = p.Country != null ? p.Country.Name : "Unknown",
                    CreatedAt = p.CreatedAt,
                    Score = p.Votes.Any() ? p.Votes.Sum(v => v.VoteType) : 0,
                    // --- Map Categories/Tags/New Fields ---
                    CategoryNames = p.PostCategories.Select(pc => pc.Category.Name).ToList(),
                    TagNames = p.PostTags.Select(pt => pt.Tag.Name).ToList(),
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    ImageUrl = p.ImageUrl // Pass ImageUrl to frontend
                    // --- End Map ---
                })
                .ToListAsync();

            return Ok(posts);
        }


        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
        // ... other responses
        public async Task<IActionResult> CreatePost(CreatePostDto createPostDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Creating post titled: {Title}", createPostDto.Title); // Log entry

            // --- PLACEHOLDER FOR MARKDOWN SANITIZATION ---
            string unsafeBody = createPostDto.Body;
            // --------------------------------------------

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Invalid user ID.");

            var countryExists = await _context.Countries.AnyAsync(c => c.Id == createPostDto.CountryId);
            if (!countryExists) return BadRequest("Selected country does not exist.");

            var post = new Post
            {
                Title = createPostDto.Title,
                Body = unsafeBody,
                CountryId = createPostDto.CountryId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                // --- Assign New Fields ---
                Latitude = createPostDto.Latitude,
                Longitude = createPostDto.Longitude,
                // ImageUrl will be set later after upload
                // --- End Assign ---
            };

            // --- Handle Categories ---
            if (createPostDto.CategoryIds?.Any() == true)
            {
                // Ensure selected categories exist (optional but safer)
                var validCategories = await _context.Categories
                    .Where(c => createPostDto.CategoryIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var catId in validCategories)
                {
                    post.PostCategories.Add(new PostCategory { CategoryId = catId });
                }
            }
            // --- End Handle Categories ---

            // --- Handle Tags ---
            if (createPostDto.TagIds?.Any() == true)
            {
                var validTags = await _context.Tags
                    .Where(t => createPostDto.TagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var tagId in validTags)
                {
                    post.PostTags.Add(new PostTag { TagId = tagId });
                }
            }
            // --- End Handle Tags ---


            _context.Posts.Add(post);
            await _context.SaveChangesAsync(); // Save post and join table entries

            // Reload related data for the response DTO
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
                CreatedAt = post.CreatedAt,
                Score = 0,
                // --- Map fields for response ---
                CategoryNames = post.PostCategories.Select(pc => pc.Category.Name).ToList(),
                TagNames = post.PostTags.Select(pt => pt.Tag.Name).ToList(),
                Latitude = post.Latitude,
                Longitude = post.Longitude,
                ImageUrl = post.ImageUrl
                // --- End Map ---
            };

            // Use GetPostById name, pass route value { id = post.Id }
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, postDto);
        }


        [HttpPost("{postId}/vote")]
        [Authorize]
        // ... method body remains the same ...

        // --- NEW: Endpoint for Image Upload ---
        [HttpPost("{postId}/image")]
        [Authorize] // Must be logged in
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadPostImage(int postId, IFormFile imageFile) // Expects file named 'imageFile'
        {
            _logger.LogInformation("Attempting image upload for Post ID: {PostId}", postId);

            // 1. Validate Input
            if (imageFile == null || imageFile.Length == 0)
            {
                _logger.LogWarning("No image file provided for Post ID: {PostId}", postId);
                return BadRequest("No image file provided.");
            }

            // Optional: Add stricter size validation
            if (imageFile.Length > 5 * 1024 * 1024) // Example: 5MB limit
            {
                _logger.LogWarning("Image file too large for Post ID: {PostId} ({Size} bytes)", postId, imageFile.Length);
                return BadRequest("Image file size exceeds the limit (e.g., 5MB).");
            }

            // Optional: Validate file type based on extension or magic bytes
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid image file type for Post ID: {PostId} ({Extension})", postId, extension);
                return BadRequest("Invalid image file type. Allowed: JPG, PNG, WEBP.");
            }

            // 2. Find the Post
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                _logger.LogWarning("Post not found for image upload: {PostId}", postId);
                return NotFound("Post not found.");
            }

            // 3. Verify Ownership (User can only upload image for their own post)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId) || post.UserId != userId)
            {
                _logger.LogWarning("User {UserId} forbidden to upload image for Post ID: {PostId} owned by User ID: {OwnerId}", userIdString, postId, post.UserId);
                return Forbid(); // 403 Forbidden
            }

            try
            {
                // 4. Define Save Path (Simple local storage)
                // NOTE: This path is relative to the *backend* server's content root.
                // In Azure App Service, this might be ephemeral. Use Blob Storage for production.
                var uploadsFolderName = "uploads"; // Name of the folder to store images
                var postImagesFolderName = $"post_{postId}"; // Subfolder per post
                var webRootPath = _environment.ContentRootPath; // Path to the API project root usually
                // If you want to serve them directly later, use _environment.WebRootPath IF static files are configured
                // For now, we save relative to the content root.

                var uploadsFolderPath = Path.Combine(webRootPath, uploadsFolderName);
                var postImagesFolderPath = Path.Combine(uploadsFolderPath, postImagesFolderName);

                // Create directories if they don't exist
                Directory.CreateDirectory(uploadsFolderPath);
                Directory.CreateDirectory(postImagesFolderPath);

                // 5. Generate Unique Filename (prevent overwrites, add security)
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(postImagesFolderPath, uniqueFileName);

                // 6. Save the File
                _logger.LogInformation("Saving image for Post ID {PostId} to {FilePath}", postId, filePath);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // 7. Update Database with Relative Path or URL
                // We'll store a relative path that needs handling on the frontend or via a backend serving endpoint.
                // A simple relative path assumes the frontend knows how to construct the full URL later.
                // Example relative path: "/uploads/post_123/guid.jpg"
                var relativeImagePath = $"/{uploadsFolderName}/{postImagesFolderName}/{uniqueFileName}".Replace("\\", "/"); // Ensure forward slashes

                // If you were serving static files from a 'wwwroot/uploads' folder:
                // var relativeImagePath = $"/{uploadsFolderName}/{postImagesFolderName}/{uniqueFileName}";

                post.ImageUrl = relativeImagePath; // Update the post entity
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved image for Post ID {PostId}. DB Path: {ImageUrl}", postId, relativeImagePath);
                return Ok(new { imageUrl = relativeImagePath }); // Return the path/URL

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for Post ID: {PostId}", postId);
                return StatusCode(500, "Internal server error during image upload.");
            }
        }
        // --- End NEW Endpoint ---




        [HttpGet("{id}")] // Ensure this exists for CreatedAtAction
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PostDto>> GetPostById(int id) // Needs to be public for CreatedAtAction

        {
            _logger.LogInformation("Getting post by ID: {PostId}", id); // Log entry

            var post = await _context.Posts
               .AsNoTracking()
               .Include(p => p.Author)
               .Include(p => p.Country)
               .Include(p => p.Votes)
               .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
               .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
               .Where(p => p.Id == id)
               .Select(p => new PostDto // Project to DTO
               {
                   Id = p.Id,
                   Title = p.Title,
                   Body = p.Body,
                   AuthorUsername = p.Author != null ? p.Author.Username : "Unknown",
                   CountryName = p.Country != null ? p.Country.Name : "Unknown",
                   CreatedAt = p.CreatedAt,
                   Score = p.Votes.Any() ? p.Votes.Sum(v => v.VoteType) : 0,
                   CategoryNames = p.PostCategories.Select(pc => pc.Category.Name).ToList(),
                   TagNames = p.PostTags.Select(pt => pt.Tag.Name).ToList(),
                   Latitude = p.Latitude,
                   Longitude = p.Longitude,
                   ImageUrl = p.ImageUrl
               })
               .FirstOrDefaultAsync();

            if (post == null)
            {
                _logger.LogWarning("Post with ID {PostId} not found.", id); // Log warning
                return NotFound();
            }

            return Ok(post);
        }
    }


}