using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismReddit.Api.Data;
using TourismReddit.Api.Models;
using TourismReddit.Api.Dtos;

namespace TourismReddit.Api.Controllers
{
    [Route("api")]
    [ApiController]
    public class CommentsController : ControllerBase
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
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ApplicationDbContext context, ILogger<CommentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

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

            var comments = await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted && c.Author != null && c.Author.IsActive)
                .Where(c => c.PostId == postId)
                .Include(c => c.Author)
                .Include(c => c.CommentVotes)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Body = c.Body ?? "[REMOVED]",
                    UserId = c.UserId ?? 0,
                    AuthorUsername = c.Author != null ? c.Author.Username : "Unknown",
                    PostId = c.PostId,
                    ParentCommentId = c.ParentCommentId,
                    CreatedAt = c.CreatedAt,
                    Score = c.CommentVotes.Any() ? c.CommentVotes.Sum(v => v.VoteType) : 0,
                })
                .ToListAsync();

            var commentMap = comments.ToDictionary(c => c.Id);
            var rootComments = new List<CommentDto>();

            foreach (var comment in comments)
            {
                if (comment.ParentCommentId.HasValue && commentMap.TryGetValue(comment.ParentCommentId.Value, out var parent))
                {
                    parent.Replies.Add(comment);
                }
                else
                {
                    rootComments.Add(comment);
                }
            }

            return Ok(rootComments);
        }

        [HttpPost("posts/{postId}/comments")]
        [Authorize]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDto>> CreateComment(int postId, CreateCommentDto createCommentDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Creating comment for Post ID: {PostId}, ParentCommentId: {ParentId}",
               postId, createCommentDto.ParentCommentId);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Invalid user ID.");

            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) return NotFound("Post not found.");

            if (createCommentDto.ParentCommentId.HasValue)
            {
                var parentExists = await _context.Comments.AnyAsync(c => c.Id == createCommentDto.ParentCommentId.Value && c.PostId == postId);
                if (!parentExists) return BadRequest("Parent comment not found or does not belong to this post.");
            }

            string unsafeBody = createCommentDto.Body;

            var comment = new Comment
            {
                Body = unsafeBody,
                PostId = postId,
                UserId = userId,
                ParentCommentId = createCommentDto.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            await _context.Entry(comment).Reference(c => c.Author).LoadAsync();

            var commentDto = new CommentDto
            {
                Id = comment.Id,
                Body = comment.Body,
                UserId = comment.UserId ?? 0,
                AuthorUsername = comment.Author?.Username ?? "Unknown",
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                CreatedAt = comment.CreatedAt,
                Score = 0,
                Replies = new List<CommentDto>()
            };

            return Created($"/api/posts/{postId}/comments/{comment.Id}", commentDto);
        }

        [HttpDelete("/api/comments/{commentId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            _logger.LogWarning("[REDACTION] Attempting redaction of Comment ID: {CommentId} by User ID: {UserId}", commentId, userId);

            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null) return NotFound("Comment not found.");

            if (comment.UserId != userId)
            {
                _logger.LogWarning("[REDACTION] User {UserId} forbidden to redact Comment ID: {CommentId} owned by User ID: {OwnerId}", userId, commentId, comment.UserId);
                return Forbid();
            }

            comment.IsDeleted = true;
            comment.Body = "[REMOVED]";
            comment.UserId = null;

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("[REDACTION] Successfully redacted Comment ID: {CommentId}", commentId);
            return NoContent();
        }

        [HttpPost("comments/{commentId}/vote")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VoteComment(int commentId, VoteDto voteDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Voting on Comment ID: {CommentId}, Direction: {Direction}", commentId, voteDto.Direction);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Unauthorized("Invalid user ID.");

            var existingVote = await _context.CommentVotes
                .FirstOrDefaultAsync(cv => cv.CommentId == commentId && cv.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.VoteType == voteDto.Direction)
                {
                    _context.CommentVotes.Remove(existingVote);
                }
                else
                {
                    existingVote.VoteType = voteDto.Direction;
                    _context.CommentVotes.Update(existingVote);
                }
            }
            else
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

            var newScore = await _context.CommentVotes
                            .Where(cv => cv.CommentId == commentId)
                            .SumAsync(cv => (int?)cv.VoteType) ?? 0;

            return Ok(new { score = newScore });
        }
    }
}