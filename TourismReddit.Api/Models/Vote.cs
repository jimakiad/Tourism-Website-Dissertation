// Filename: Models/Vote.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models // Ensure namespace matches
{
    public class Vote
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; } // FK
        [Required]
        public int PostId { get; set; } // FK
        [Required]
        public int VoteType { get; set; } // 1 for like, -1 for dislike

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; } // Nullable reference type enabled? Use User? otherwise User
        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; } // Nullable reference type enabled? Use Post? otherwise Post
    }
}