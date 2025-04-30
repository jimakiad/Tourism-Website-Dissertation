// Models/Post.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models;

public class Post
{
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Body { get; set; } = string.Empty;
    [Required]
    public int CountryId { get; set; }

    // --- New Fields ---
    public double? Latitude { get; set; } // Optional latitude
    public double? Longitude { get; set; } // Optional longitude
    public string? ImageUrl { get; set; } // Store path/URL to image (simple approach)
    // --- End New Fields ---

    // --- Remove simple string fields ---
    // public string Tags { get; set; } = string.Empty;
    // public string Categories { get; set; } = string.Empty;
    // --- End Remove ---

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? Author { get; set; }
    [ForeignKey("CountryId")]
    public virtual Country? Country { get; set; }
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

    // --- Add Relationships for Categories/Tags ---
    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    // --- End Add Relationships ---

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [NotMapped]
    public int Score => Votes?.Any() == true ? Votes.Sum(v => v.VoteType) : 0;
}