using System.ComponentModel.DataAnnotations;

namespace TourismReddit.Api.Dtos
{
    public class CreatePostDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(300, ErrorMessage = "Title cannot exceed 300 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Body content is required")]
        public string Body { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country selection is required")]
        public int CountryId { get; set; }

        [MaxLength(200)]
        public string? Tags { get; set; }

        [MaxLength(200)]
        public string? Categories { get; set; }
    }
}