using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models;

public class CommentVote
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; } 

    [Required]
    public int CommentId { get; set; } 

    [Required]
    public int VoteType { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("CommentId")]
    public virtual Comment? Comment { get; set; }
}