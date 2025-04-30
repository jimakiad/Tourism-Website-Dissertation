// Models/Comment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string Body { get; set; } = string.Empty; // Markdown or plain text - Sanitize!

    [Required]
    public int UserId { get; set; } // Author of the comment

    [Required]
    public int PostId { get; set; } // Post this comment belongs to

    // --- For Nested Replies ---
    public int? ParentCommentId { get; set; } // Null if top-level, otherwise ID of parent
    // --- End Nested Replies ---

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User? Author { get; set; }

    [ForeignKey("PostId")]
    public virtual Post? Post { get; set; }

    // --- For Nested Replies ---
    [ForeignKey("ParentCommentId")]
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>(); // Children comments
    // --- End Nested Replies ---

    // --- Add Votes for Comments ---
    public virtual ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
    // --- End Add Votes ---

    // --- Calculated Score for Comment ---
    [NotMapped]
    public int Score => CommentVotes.Any() ? CommentVotes.Sum(v => v.VoteType) : 0;
    // --- End Calculated Score ---
}