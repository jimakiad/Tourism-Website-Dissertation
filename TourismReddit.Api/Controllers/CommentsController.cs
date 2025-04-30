// Controllers/CommentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismReddit.Api.Data;
using TourismReddit.Api.Models;
using TourismReddit.Api.Dtos;

namespace TourismReddit.Api.Controllers
{
    [Route("api")] // Base route for flexibility
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ApplicationDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- GET Comments for a Post ---
        // GET: /api/posts/{postId}/comments
        [HttpGet("posts/{postId}/comments")]
        [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetPostComments(int postId)
        {
            _logger.LogInformation("Getting comments for Post ID: {PostId}", postId);

            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
            {
                _logger.LogWarning("Post not found when getting comments: {PostId}", postId);
                return NotFound("Post not found.");
            }

            // Fetch all comments for the post, including author and vote info
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.Author)
                .Include(c => c.CommentVotes)
                // Include replies recursively? This can get complex.
                // For simplicity now, we fetch all and reconstruct hierarchy later or fetch level by level.
                // Let's fetch all and mark ParentCommentId
                .OrderBy(c => c.CreatedAt) // Order chronologically
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Body = c.Body,
                    UserId = c.UserId,
                    AuthorUsername = c.Author != null ? c.Author.Username : "Unknown",
                    PostId = c.PostId,
                    ParentCommentId = c.ParentCommentId,
                    CreatedAt = c.CreatedAt,
                    Score = c.CommentVotes.Any() ? c.CommentVotes.Sum(v => v.VoteType) : 0,
                    // Replies = new List<CommentDto>() // Initialize empty, hierarchy built later
                })
                .ToListAsync();

            // --- Simple Hierarchy Reconstruction (for one level deep for now) ---
            // In a real app, use a more robust recursive function or fetch differently
            var commentMap = comments.ToDictionary(c => c.Id);
            var rootComments = new List<CommentDto>();

            foreach (var comment in comments)
            {
                if (comment.ParentCommentId.HasValue && commentMap.TryGetValue(comment.ParentCommentId.Value, out var parent))
                {
                    parent.Replies.Add(comment); // Add to parent's reply list
                }
                else
                {
                    rootComments.Add(comment); // Add top-level comments
                }
            }
            // --- End Simple Hierarchy ---

            return Ok(rootComments); // Return only top-level comments (with nested replies)
        }


        // --- POST a new Comment ---
        // POST: /api/posts/{postId}/comments
        [HttpPost("posts/{postId}/comments")]
        [Authorize] // Must be logged in
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDto>> CreateComment(int postId, CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Creating comment for Post ID: {PostId}, ParentCommentId: {ParentId}",
               postId, createCommentDto.ParentCommentId);

            // Get User ID
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Invalid user ID.");

            // Check if Post exists
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) return NotFound("Post not found.");

            // Optional: Check if Parent Comment exists if ParentCommentId is provided
            if (createCommentDto.ParentCommentId.HasValue)
            {
                var parentExists = await _context.Comments.AnyAsync(c => c.Id == createCommentDto.ParentCommentId.Value && c.PostId == postId);
                if (!parentExists) return BadRequest("Parent comment not found or does not belong to this post.");
            }

            // --- Sanitize Comment Body Here ---
            // string sanitizedBody = SanitizeMarkdown(createCommentDto.Body);
            string unsafeBody = createCommentDto.Body; // Needs sanitization!
            // --- End Sanitization ---

            var comment = new Comment
            {
                Body = unsafeBody, // Use sanitized body
                PostId = postId,
                UserId = userId,
                ParentCommentId = createCommentDto.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Load Author for the response DTO
            await _context.Entry(comment).Reference(c => c.Author).LoadAsync();

            var commentDto = new CommentDto
            {
                Id = comment.Id,
                Body = comment.Body,
                UserId = comment.UserId,
                AuthorUsername = comment.Author?.Username ?? "Unknown",
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt,
                Score = 0, // New comment
                Replies = new List<CommentDto>() // No replies yet
            };

            // Consider how to return this - maybe just Ok(commentDto) is simpler than CreatedAtAction
            return Created($"/api/posts/{postId}/comments/{comment.Id}", commentDto); // Needs a GetCommentById endpoint
                                                                                      // return Ok(commentDto); // Simpler alternative
        }


        // --- POST Vote on a Comment ---
        // POST: /api/comments/{commentId}/vote
        [HttpPost("comments/{commentId}/vote")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Returns { score: newScore }
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VoteComment(int commentId, VoteDto voteDto) // Reuse VoteDto from Posts
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Voting on Comment ID: {CommentId}, Direction: {Direction}", commentId, voteDto.Direction);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Invalid user ID.");

            // Find existing vote by this user for this comment
            var existingVote = await _context.CommentVotes
                .FirstOrDefaultAsync(cv => cv.CommentId == commentId && cv.UserId == userId);

            if (existingVote != null) // Vote exists
            {
                if (existingVote.VoteType == voteDto.Direction)
                {
                    _context.CommentVotes.Remove(existingVote); // Unvote
                }
                else
                {
                    existingVote.VoteType = voteDto.Direction; // Change vote
                    _context.CommentVotes.Update(existingVote);
                }
            }
            else // No existing vote
            {
                var commentExists = await _context.Comments.AnyAsync(c => c.Id == commentId);
                if (!commentExists) return NotFound("Comment not found.");

                var newVote = new CommentVote
                {
                    UserId = userId,
                    CommentId = commentId,
                    VoteType = voteDto.Direction
                };
                _context.CommentVotes.Add(newVote);
            }

            await _context.SaveChangesAsync();

            // Recalculate score
            var newScore = await _context.CommentVotes
                            .Where(cv => cv.CommentId == commentId)
                            .SumAsync(cv => (int?)cv.VoteType) ?? 0;

            return Ok(new { score = newScore });
        }

        // Add GET /api/comments/{id} if needed for CreatedAtAction route or direct fetching
        // Add PUT /api/comments/{id} for editing
        // Add DELETE /api/comments/{id} for deleting
    }
}