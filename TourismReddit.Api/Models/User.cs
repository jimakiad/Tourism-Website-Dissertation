using System.ComponentModel.DataAnnotations;

namespace TourismReddit.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSubscribed { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

    }
}