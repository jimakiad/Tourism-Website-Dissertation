using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismReddit.Api.Models 
{
    public class Vote
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int PostId { get; set; }
        [Required]
        public int VoteType { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; } 
        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; } 
    }
}