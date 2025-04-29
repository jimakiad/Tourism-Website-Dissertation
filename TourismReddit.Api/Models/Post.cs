// Filename: Models/Post.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models // Ensure namespace matches
{
    public class Post
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; } // FK
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty; // Plain text
        [Required]
        public string Body { get; set; } = string.Empty; // Markdown - SANITIZE ON SAVE
        [Required]
        public int CountryId { get; set; } // FK
        public string Tags { get; set; } = string.Empty; // Simple comma-separated for now
        public string Categories { get; set; } = string.Empty; // Simple comma-separated for now
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? Author { get; set; } // Nullable reference type enabled? Use User? otherwise User
        [ForeignKey("CountryId")]
        public virtual Country? Country { get; set; } // Nullable reference type enabled? Use Country? otherwise Country
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

        [NotMapped] // Calculated property, not stored in DB
        public int Score => Votes?.Any() == true ? Votes.Sum(v => v.VoteType) : 0; // Add null check for safety
    }
}