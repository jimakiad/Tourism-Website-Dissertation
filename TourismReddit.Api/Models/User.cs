// Filename: Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace TourismReddit.Api.Models // Make sure this namespace matches your project name
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty; // Add validation later
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSubscribed { get; set; } = false;
        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        
    }
}