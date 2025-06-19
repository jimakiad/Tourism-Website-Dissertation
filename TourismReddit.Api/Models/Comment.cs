using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string? Body { get; set; } = string.Empty;

    [Required]
    public int? UserId { get; set; }

    [Required]
    public int PostId { get; set; }

    public int? ParentCommentId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual User? Author { get; set; }

    [ForeignKey("PostId")]
    public virtual Post? Post { get; set; }

    [ForeignKey("ParentCommentId")]
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public virtual ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();

    [NotMapped]
    public int Score => CommentVotes.Any() ? CommentVotes.Sum(v => v.VoteType) : 0;

}