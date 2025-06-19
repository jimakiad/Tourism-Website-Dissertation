using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models;

public class Post
{
    public int Id { get; set; }
    [Required]
    public int? UserId { get; set; }
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string? Body { get; set; } = string.Empty;
    [Required]
    public int CountryId { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ImageUrl { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UserId")]
    public virtual User? Author { get; set; }
    [ForeignKey("CountryId")]
    public virtual Country? Country { get; set; }
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [NotMapped]
    public int Score => Votes?.Any() == true ? Votes.Sum(v => v.VoteType) : 0;

}