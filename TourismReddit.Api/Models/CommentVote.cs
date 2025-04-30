// Models/CommentVote.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models;

public class CommentVote
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; } // User who voted

    [Required]
    public int CommentId { get; set; } // Comment being voted on

    [Required]
    public int VoteType { get; set; } // 1 for like, -1 for dislike

    // Navigation Properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("CommentId")]
    public virtual Comment? Comment { get; set; }
}