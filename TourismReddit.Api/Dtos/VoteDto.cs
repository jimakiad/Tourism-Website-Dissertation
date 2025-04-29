using System.ComponentModel.DataAnnotations;

namespace TourismReddit.Api.Dtos
{
    public class VoteDto
    {
        [Required]
        [Range(-1, 1, ErrorMessage = "Vote direction must be 1 (like) or -1 (dislike)")]
        public int Direction { get; set; }
    }
}