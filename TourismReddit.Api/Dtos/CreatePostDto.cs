using System.ComponentModel.DataAnnotations;

namespace TourismReddit.Api.Dtos
{
    public class CreatePostDto
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Body { get; set; } = string.Empty;
        [Required]
        public int CountryId { get; set; }
        public List<int>? CategoryIds { get; set; } 
        public List<int>? TagIds { get; set; } 
        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double? Latitude { get; set; }
        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double? Longitude { get; set; }
    }
}